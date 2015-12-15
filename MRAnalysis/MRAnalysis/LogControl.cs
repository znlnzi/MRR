using System.Collections.Generic;
using System.Windows.Forms;
using MRAnalysis.Model;

namespace MRAnalysis
{
    public partial class LogControl : UserControl
    {
        //public BindingSource LogEntityBindingSource;

        public IList<Log> LogEntities
        {
            get
            {
                return (IList<Log>)LogEntityBindingSource.List;

            }
            set
            {
                LogEntityBindingSource.Add(value);

            }
        }

        public LogControl()
        {
            InitializeComponent();
        }
    }
}
