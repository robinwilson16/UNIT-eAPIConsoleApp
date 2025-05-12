using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSuiteIntegration.Interfaces
{
    public interface IFinanceWebService
    {
        //Utility methods

        /// <summary>
        /// Gets a record from NetSuite by its Type and ID and returns the object
        /// </summary>
        /// <typeparam name="T">A custom data model representing the expected return structure</typeparam>
        /// <param name="recordType">The type of object to be returned</param>
        /// <param name="recordID">The id of the record</param>
        /// <returns></returns>

        Task<T?> GetNetSuiteRecord<T>(string? recordType, int? recordID);
    }
}
