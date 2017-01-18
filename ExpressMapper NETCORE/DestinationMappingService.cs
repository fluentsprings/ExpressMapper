using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressMapper
{
    internal class DestinationMappingService : MappingServiceBase, IMappingService
    {
        #region Constructors

        public DestinationMappingService(IMappingServiceProvider mappingServiceProvider)
            : base(mappingServiceProvider)
        {
        }

        #endregion

        #region Privates

        private readonly IDictionary<long, MulticastDelegate> _collectionMappers =
            new Dictionary<long, MulticastDelegate>();

        #endregion

        #region Implementation of IMappingService

        public override IDictionary<long, MulticastDelegate> CollectionMappers => _collectionMappers;

        public override bool DestinationSupport => true;

        public override MulticastDelegate MapCollection(long cacheKey)
        {
            return CollectionMappers.ContainsKey(cacheKey) ? CollectionMappers[cacheKey] : null;
        }

        public override BlockExpression MapCollection(Type srcColtype, Type destColType, Expression srcExpression, Expression destExpression)
        {
            var sourceType = GetCollectionElementType(srcColtype);
            var destType = GetCollectionElementType(destColType);
            var sourceVariable = Expression.Variable(srcExpression.Type,
                $"{Guid.NewGuid().ToString().Replace("-", "_")}Src");
            var assignSourceVarExp = Expression.Assign(sourceVariable, srcExpression);

            var srcCount = Expression.Call(typeof(Enumerable), "Count", new[] { sourceType }, sourceVariable);
            var destCount = Expression.Call(typeof(Enumerable), "Count", new[] { destType }, destExpression);

            var conditionToCreateList = Expression.NotEqual(srcCount, destCount);
            var notNullCondition = Expression.IfThenElse(conditionToCreateList,
                MapCollectionNotCountEquals(srcExpression.Type, destExpression.Type, srcExpression,
                    destExpression),
                MapCollectionCountEquals(srcColtype, destColType, srcExpression, destExpression));

            var result = Expression.IfThenElse(Expression.NotEqual(destExpression, StaticExpressions.NullConstant), notNullCondition,
                base.MapCollection(srcColtype, destColType, srcExpression, destExpression));

            var expressions = new List<Expression> { assignSourceVarExp, result };

            var resultExpression = Expression.Block(new[] { sourceVariable }, expressions);

            var checkSrcForNullExp =
                Expression.IfThenElse(Expression.Equal(srcExpression, StaticExpressions.NullConstant),
                    Expression.Assign(destExpression, Expression.Default(destExpression.Type)), resultExpression);
            var block = Expression.Block(new ParameterExpression[] { }, new Expression[] { checkSrcForNullExp });

            return block;
        }

        public override BlockExpression MapProperty(Type srcType, Type destType, Expression srcExpression, Expression destExpression, bool newDest)
        {
            var sourceVariable = Expression.Variable(srcType,
                $"{srcType.Name}_{Guid.NewGuid().ToString().Replace("-", "_")}Src");

            var assignSourceFromProp = Expression.Assign(sourceVariable, srcExpression);

            var mapExpressions = GetMapExpressions(srcType, destType);
            var mapExprForType = mapExpressions.Item1;
            var destVariable = Expression.Variable(destType,
                $"{destType.Name}_{Guid.NewGuid().ToString().Replace("-", "_")}Dst");
            var assignDestFromProp = Expression.Assign(destVariable, destExpression);

            var ifDestNull = destType.GetInfo().IsPrimitive || destType.GetInfo().IsEnum ? (Expression)StaticExpressions.FalseConstant : Expression.Equal(destExpression, StaticExpressions.NullConstant);

            var newDestInstanceExp = mapExprForType[0] as BinaryExpression;
            if (newDestInstanceExp != null && !newDest)
            {
                mapExprForType.RemoveAt(0);

                var destVar = newDestInstanceExp.Left as ParameterExpression;

                var assignExistingDestExp = Expression.Assign(destVar, destExpression);

                var destCondition = Expression.IfThenElse(ifDestNull, newDestInstanceExp, assignExistingDestExp);
                mapExprForType.Insert(0, destCondition);
            }

            var blockForSubstitution = Expression.Block(mapExprForType);
            var substBlock =
                new PreciseSubstituteParameterVisitor(
                            new KeyValuePair<ParameterExpression, ParameterExpression>(mapExpressions.Item2, sourceVariable),
                            new KeyValuePair<ParameterExpression, ParameterExpression>(mapExpressions.Item3, destVariable))
                        .Visit(blockForSubstitution) as
                    BlockExpression;

            //var substBlock =
            //    new SubstituteParameterVisitor(sourceVariable, destVariable).Visit(blockForSubstitution) as
            //        BlockExpression;
            var resultMapExprForType = substBlock.Expressions;

            var assignExp = Expression.Assign(destExpression, destVariable);

            var expressions = new List<Expression> { assignSourceFromProp };
            if (!newDest)
            {
                expressions.Add(assignDestFromProp);
            }
            expressions.AddRange(resultMapExprForType);
            expressions.Add(assignExp);

            var parameterExpressions = new List<ParameterExpression> { sourceVariable, destVariable };
            var blockExpression = Expression.Block(parameterExpressions, expressions);

            return blockExpression;
        }

        public Expression GetMemberQueryableExpression(Type srcType, Type dstType)
        {
            throw new NotImplementedException();
        }

        public override void PrecompileCollection<T, TN>()
        {
            var cacheKey = MappingServiceProvider.CalculateCacheKey(typeof(T), typeof(TN));
            if (CollectionMappers.ContainsKey(cacheKey)) return;

            var sourceType = GetCollectionElementType(typeof(T));
            var destType = GetCollectionElementType(typeof(TN));

            var sourceVariable = Expression.Parameter(typeof(T), "source");
            var destVariable = Expression.Parameter(typeof(TN), "dst");

            var srcCount = Expression.Call(typeof(Enumerable), "Count", new[] { sourceType }, sourceVariable);
            var destCount = Expression.Call(typeof(Enumerable), "Count", new[] { destType }, destVariable);

            var conditionToCreateList = Expression.NotEqual(srcCount, destCount);
            var notNullCondition = Expression.IfThenElse(conditionToCreateList,
                MapCollectionNotCountEquals(typeof(T), typeof(TN), sourceVariable, destVariable),
                MapCollectionCountEquals(typeof(T), typeof(TN), sourceVariable, destVariable));

            var newCollBlockExp = CompileCollectionInternal<T, TN>(sourceVariable, destVariable);

            var result = Expression.IfThenElse(Expression.NotEqual(destVariable, StaticExpressions.NullConstant),
                notNullCondition,
                newCollBlockExp);

            var expressions = new List<Expression> { result };

            var resultExpression = Expression.Block(new ParameterExpression[] { }, expressions);

            var checkSrcForNullExp =
                Expression.IfThenElse(Expression.Equal(sourceVariable, StaticExpressions.NullConstant),
                    Expression.Default(destVariable.Type), resultExpression);
            var block = Expression.Block(new ParameterExpression[] { }, checkSrcForNullExp, destVariable);
            var lambda = Expression.Lambda<Func<T, TN, TN>>(block, sourceVariable, destVariable);
            var compiledFunc = lambda.Compile();

            CollectionMappers[cacheKey] = compiledFunc;
        }

        #endregion

        #region Helper methods

        private BlockExpression MapCollectionNotCountEquals(Type tCol, Type tnCol, Expression sourceVariable,
            Expression destVariable)
        {
            var sourceType = GetCollectionElementType(tCol);
            var destType = GetCollectionElementType(tnCol);

            var destList = typeof(List<>).MakeGenericType(destType);
            var destCollection = typeof(ICollection<>).MakeGenericType(destType);

            BlockExpression resultExpression;
            var isICollection = !destVariable.Type.IsArray && (destVariable.Type.GetInfo().GetInterfaces()
                                                                   .FirstOrDefault(t => t.GetInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>)) != null ||
                                                               destVariable.Type == destCollection);

            var srcCount = Expression.Call(typeof(Enumerable), "Count", new[] { sourceType }, sourceVariable);
            var destCount = Expression.Call(typeof(Enumerable), "Count", new[] { destType }, destVariable);

            if (isICollection)
            {
                // If it is a list and destCount greater than srcCount

                var equalsBlockExp = MapCollectionCountEquals(tCol, tnCol, sourceVariable, destVariable);

                var getFirstEnumExp = Expression.Call(typeof(Enumerable), "First", new[] { destType }, destVariable);
                var removeCollFirstExp = Expression.Call(destVariable, destCollection.GetInfo().GetMethod("Remove"),
                    getFirstEnumExp);

                var brkColRem = Expression.Label();
                var loopToDropColElements = Expression.Loop(
                    Expression.IfThenElse(Expression.GreaterThan(destCount, srcCount),
                        removeCollFirstExp
                        , Expression.Break(brkColRem))
                    , brkColRem);

                var collRemoveExps = new List<Expression> { loopToDropColElements, equalsBlockExp };
                var collRemoveBlockExp = Expression.Block(collRemoveExps);

                // List and Collection - if src count greater than dest

                var mapCollectionSourcePrevail = MapCollectionSourcePrevail(destVariable, sourceType, sourceVariable,
                    destType);
                var collBlock = Expression.IfThenElse(Expression.GreaterThan(destCount, srcCount), collRemoveBlockExp,
                    mapCollectionSourcePrevail);
                resultExpression = Expression.Block(new ParameterExpression[] { }, new Expression[] { collBlock });
            }
            else
            {
                // Else

                var destListType = typeof(List<>).MakeGenericType(destType);
                var destVarExp = Expression.Variable(destListType, $"{Guid.NewGuid().ToString("N")}InterimDst");
                var constructorInfo = destListType.GetInfo().GetConstructor(new Type[] { typeof(int) });

                var newColl = Expression.New(constructorInfo, srcCount);
                var destAssign = Expression.Assign(destVarExp, newColl);

                // Source enumeration
                var closedEnumeratorSourceType = typeof(IEnumerator<>).MakeGenericType(sourceType);
                var closedEnumerableSourceType = GenericEnumerableType.MakeGenericType(sourceType);
                var enumeratorSrc = Expression.Variable(closedEnumeratorSourceType,
                    $"{Guid.NewGuid().ToString("N")}EnumSrc");
                var assignToEnumSrc = Expression.Assign(enumeratorSrc,
                    Expression.Call(sourceVariable, closedEnumerableSourceType.GetInfo().GetMethod("GetEnumerator")));
                var doMoveNextSrc = Expression.Call(enumeratorSrc, typeof(IEnumerator).GetInfo().GetMethod("MoveNext"));
                var currentSrc = Expression.Property(enumeratorSrc, "Current");

                var srcItmVarExp = Expression.Variable(sourceType, $"{Guid.NewGuid().ToString("N")}ItmSrc");
                var assignSourceItmFromProp = Expression.Assign(srcItmVarExp, currentSrc);

                // dest enumeration
                var closedEnumeratorDestType = typeof(IEnumerator<>).MakeGenericType(destType);
                var closedEnumerableDestType = GenericEnumerableType.MakeGenericType(destType);
                var enumeratorDest = Expression.Variable(closedEnumeratorDestType,
                    $"{Guid.NewGuid():N}EnumDst");
                var assignToEnumDest = Expression.Assign(enumeratorDest,
                    Expression.Call(destVariable, closedEnumerableDestType.GetInfo().GetMethod("GetEnumerator")));
                var doMoveNextDest = Expression.Call(enumeratorDest, typeof(IEnumerator).GetInfo().GetMethod("MoveNext"));

                var currentDest = Expression.Property(enumeratorDest, "Current");
                var destItmVarExp = Expression.Variable(destType, $"{Guid.NewGuid().ToString("N")}ItmDst");
                var assignDestItmFromProp = Expression.Assign(destItmVarExp, currentDest);


                // If destination list is empty
                var mapExprForType = GetMemberMappingExpression(destItmVarExp, srcItmVarExp, true);

                var ifTrueBlock = IfElseExpr(srcItmVarExp, destItmVarExp, assignDestItmFromProp);

                var mapAndAddItemExp = Expression.IfThenElse(doMoveNextDest, ifTrueBlock, mapExprForType);
                var addToNewCollNew = Expression.Call(destVarExp, "Add", null, destItmVarExp);

                var innerLoopBlock = Expression.Block(new[] { srcItmVarExp, destItmVarExp },
                    new Expression[] { assignSourceItmFromProp, mapAndAddItemExp, addToNewCollNew });

                var loopExpression = CreateLoopExpression(doMoveNextSrc, innerLoopBlock);

                var resultCollection = ConvertCollection(destVariable.Type, destList, destType, destVarExp);

                var assignResult = Expression.Assign(destVariable, resultCollection);

                var parameters = new List<ParameterExpression> { destVarExp, enumeratorSrc, enumeratorDest };
                var expressions = new List<Expression>
                {
                    destAssign,
                    assignToEnumSrc,
                    assignToEnumDest,
                    loopExpression,
                    assignResult
                };

                resultExpression = Expression.Block(parameters, expressions);
            }

            return resultExpression;
        }

        private BlockExpression MapCollectionCountEquals(Type tCol, Type tnCol, Expression sourceVariable,
            Expression destVariable)
        {
            var sourceType = GetCollectionElementType(tCol);
            var destType = GetCollectionElementType(tnCol);

            // Source enumeration
            var closedEnumeratorSourceType = typeof(IEnumerator<>).MakeGenericType(sourceType);
            var closedEnumerableSourceType = GenericEnumerableType.MakeGenericType(sourceType);
            var enumeratorSrc = Expression.Variable(closedEnumeratorSourceType, $"{Guid.NewGuid().ToString("N")}EnumSrc");
            var assignToEnumSrc = Expression.Assign(enumeratorSrc,
                Expression.Call(sourceVariable, closedEnumerableSourceType.GetInfo().GetMethod("GetEnumerator")));
            var doMoveNextSrc = Expression.Call(enumeratorSrc, typeof(IEnumerator).GetInfo().GetMethod("MoveNext"));
            var currentSrc = Expression.Property(enumeratorSrc, "Current");
            var srcItmVarExp = Expression.Variable(sourceType, $"{Guid.NewGuid().ToString("N")}ItmSrc");
            var assignSourceItmFromProp = Expression.Assign(srcItmVarExp, currentSrc);

            // dest enumeration
            var closedEnumeratorDestType = typeof(IEnumerator<>).MakeGenericType(destType);
            var closedEnumerableDestType = GenericEnumerableType.MakeGenericType(destType);
            var enumeratorDest = Expression.Variable(closedEnumeratorDestType, $"{Guid.NewGuid().ToString("N")}EnumDst");
            var assignToEnumDest = Expression.Assign(enumeratorDest,
                Expression.Call(destVariable, closedEnumerableDestType.GetInfo().GetMethod("GetEnumerator")));
            var doMoveNextDest = Expression.Call(enumeratorDest, typeof(IEnumerator).GetInfo().GetMethod("MoveNext"));
            var currentDest = Expression.Property(enumeratorDest, "Current");
            var destItmVarExp = Expression.Variable(destType, $"{Guid.NewGuid().ToString("N")}ItmDst");
            var assignDestItmFromProp = Expression.Assign(destItmVarExp, currentDest);

            var mapExprForType = GetMemberMappingExpression(destItmVarExp, srcItmVarExp, false);

            var ifTrueBlock = Expression.Block(new[] { srcItmVarExp, destItmVarExp },
                new[] { assignSourceItmFromProp, assignDestItmFromProp, mapExprForType });

            var brk = Expression.Label();
            var loopExpression = Expression.Loop(
                Expression.IfThenElse(
                    Expression.AndAlso(Expression.NotEqual(doMoveNextSrc, StaticExpressions.FalseConstant),
                        Expression.NotEqual(doMoveNextDest, StaticExpressions.FalseConstant)),
                    ifTrueBlock
                    , Expression.Break(brk))
                , brk);

            var parameters = new List<ParameterExpression> { enumeratorSrc, enumeratorDest };
            var expressions = new List<Expression>
            {
                assignToEnumSrc,
                assignToEnumDest,
                loopExpression
            };

            var blockExpression = Expression.Block(parameters, expressions);
            return blockExpression;
        }

        private BlockExpression MapCollectionSourcePrevail(Expression destVariable, Type sourceType,
            Expression sourceVariable, Type destType)
        {
            // Source enumeration
            var closedEnumeratorSourceType = typeof(IEnumerator<>).MakeGenericType(sourceType);
            var closedEnumerableSourceType = GenericEnumerableType.MakeGenericType(sourceType);
            var enumeratorSrc = Expression.Variable(closedEnumeratorSourceType, $"{Guid.NewGuid().ToString("N")}EnumSrc");
            var assignToEnumSrc = Expression.Assign(enumeratorSrc,
                Expression.Call(sourceVariable, closedEnumerableSourceType.GetInfo().GetMethod("GetEnumerator")));
            var doMoveNextSrc = Expression.Call(enumeratorSrc, typeof(IEnumerator).GetInfo().GetMethod("MoveNext"));
            var currentSrc = Expression.Property(enumeratorSrc, "Current");

            var srcItmVarExp = Expression.Variable(sourceType, $"{Guid.NewGuid().ToString("N")}ItmSrc");
            var assignSourceItmFromProp = Expression.Assign(srcItmVarExp, currentSrc);

            // dest enumeration
            var closedEnumeratorDestType = typeof(IEnumerator<>).MakeGenericType(destType);
            var closedEnumerableDestType = GenericEnumerableType.MakeGenericType(destType);
            var enumeratorDest = Expression.Variable(closedEnumeratorDestType, $"{Guid.NewGuid().ToString("N")}EnumDst");
            var assignToEnumDest = Expression.Assign(enumeratorDest,
                Expression.Call(destVariable, closedEnumerableDestType.GetInfo().GetMethod("GetEnumerator")));
            var doMoveNextDest = Expression.Call(enumeratorDest, typeof(IEnumerator).GetInfo().GetMethod("MoveNext"));

            var currentDest = Expression.Property(enumeratorDest, "Current");
            var destItmVarExp = Expression.Variable(destType, $"{Guid.NewGuid().ToString("N")}ItmDst");
            var assignDestItmFromProp = Expression.Assign(destItmVarExp, currentDest);

            var ifTrueBlock = IfElseExpr(srcItmVarExp, destItmVarExp, assignDestItmFromProp);

            // If destination list is empty
            var mapExprForType = GetMemberMappingExpression(destItmVarExp, srcItmVarExp, true);

            var destCollection = typeof(ICollection<>).MakeGenericType(destType);

            var addToNewCollNew = Expression.Call(destVariable, destCollection.GetInfo().GetMethod("Add"), destItmVarExp);

            var ifFalseBlock = Expression.Block(new ParameterExpression[] { }, new[] { mapExprForType, addToNewCollNew });

            var endOfListExp = Expression.Variable(typeof(bool), "endOfList");
            var assignInitEndOfListExp = Expression.Assign(endOfListExp, StaticExpressions.FalseConstant);

            var ifNotEndOfListExp = Expression.IfThen(Expression.Equal(endOfListExp, StaticExpressions.FalseConstant),
                Expression.Assign(endOfListExp, Expression.Not(doMoveNextDest)));

            var mapAndAddItemExp = Expression.IfThenElse(endOfListExp, ifFalseBlock, ifTrueBlock);

            var innerLoopBlock = Expression.Block(new[] { srcItmVarExp, destItmVarExp },
                new Expression[] { assignSourceItmFromProp, ifNotEndOfListExp, mapAndAddItemExp });

            var loopExpression = CreateLoopExpression(doMoveNextSrc, innerLoopBlock);

            var blockExpression = Expression.Block(new[] { endOfListExp, enumeratorSrc, enumeratorDest },
                new Expression[] { assignInitEndOfListExp, assignToEnumSrc, assignToEnumDest, loopExpression });
            return blockExpression;
        }

        private Expression IfElseExpr(Expression srcItmVarExp,
            Expression destItmVarExp,
            Expression assignDestItmFromProp)
        {
            // TODO: Change name
            var mapExprForType = GetMemberMappingExpression(destItmVarExp, srcItmVarExp, false);

            return Expression.Block(new ParameterExpression[] { }, new[] { assignDestItmFromProp, mapExprForType });
        }

        #endregion
    }
}