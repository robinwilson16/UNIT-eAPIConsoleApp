using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSuiteIntegration.Interfaces
{
    public interface IProcessService
    {
        Task<bool> Process(string? enrolmentRepGen, string? courseRepGen, bool? readOnly, bool? firstRecordOnly);
    }
}
