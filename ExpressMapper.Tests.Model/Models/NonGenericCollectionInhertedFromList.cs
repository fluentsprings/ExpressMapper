using System.Collections.Generic;

using ExpressMapper.Tests.Model.ViewModels;

namespace ExpressMapper.Tests.Model.Models {
  public class NonGenericCollectionInhertedFromList: List<TestViewModel> {

    public NonGenericCollectionInhertedFromList( IEnumerable<TestViewModel> models ) : base( models ) { }
  }
}