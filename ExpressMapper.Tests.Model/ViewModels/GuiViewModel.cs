using System.Collections.Generic;

namespace ExpressMapper.Tests.Model.ViewModels
{
    public class GuiViewModel
    {
        public IEnumerable<BaseControlViewModel> ControlViewModels { get; set; }
    }
}