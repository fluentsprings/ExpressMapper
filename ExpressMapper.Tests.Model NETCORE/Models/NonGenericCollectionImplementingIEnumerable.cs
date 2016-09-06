using System.Collections;
using System.Collections.Generic;
using System.Linq;

using ExpressMapper.Tests.Model.ViewModels;

namespace ExpressMapper.Tests.Model.Models {
  public class NonGenericCollectionImplementingIEnumerable: IEnumerable<TestViewModel> {
    private readonly List<TestViewModel> _models;

    public NonGenericCollectionImplementingIEnumerable( IEnumerable<TestViewModel> models ) {
      _models = models.ToList();
    }

    public IEnumerator<TestViewModel> GetEnumerator() {
      return _models.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }
}