using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetSuiteIntegration.Models;

namespace NetSuiteIntegration.Interfaces
{
    public interface IFinanceWebService
    {
        //Utility methods

        /// <summary>
        /// Gets a record from NetSuite by its Type and ID and returns the object
        /// </summary>
        /// <typeparam name="T">A custom data model representing the expected return structure</typeparam>
        /// <param name="objectType">The type of object to be returned</param>
        /// <param name="objectID">The id of the record</param>
        /// <returns>A record of the specified type, else null</returns>
        Task<T?> Get<T>(string? objectType, int? objectID);

        /// <summary>
        /// Gets a set of records from NetSuite by their Type and returns the set of objects
        /// </summary>
        /// <typeparam name="T">A custom data model representing the expected return structure</typeparam>
        /// <param name="objectType">The type of object to be returned</param>
        /// <returns>A set of records of the specified type, else null</returns>
        Task<T?> GetAll<T>(string? objectType);

        /// <summary>
        /// Gets a set of records from NetSuite by their Type and returns the search list which is not the full list of items and only the ID
        /// </summary>
        /// <typeparam name="T">A custom data model representing the expected return structure</typeparam>
        /// <param name="objectType">The type of object to be returned</param>
        /// <param name="searchParameters">The list of search parameters for filtering the resultset</param>
        /// <returns>A set of records of the specified type, else null</returns>
        Task<T?> Search<T>(string? objectType, IList<NetSuiteSearchParameter> searchParameters);

        /// <summary>
        /// Gets a set of records from NetSuite by their Type and returns either all items or a filtered set of items based on the SQL query provided.
        /// </summary>
        /// <typeparam name="T">A custom data model representing the expected return structure</typeparam>
        /// <param name="objectType">The type of object to be returned</param>
        /// <param name="sqlQuery">The SQL query to select either specific fields or all fields and may contain a WHERE clause to filter the resultset</param>
        /// <returns>A set of records of the specified type, else null</returns>
        Task<T?> SearchSQL<T>(string? objectType, NetSuiteSQLQuery sqlQuery);

        /// <summary>
        /// Adds a record to NetSuite by its Type and returns the newly added object
        /// </summary>
        /// <typeparam name="T">A custom data model representing the expected return structure</typeparam>
        /// <param name="objectType">The type of object to be added</param>
        /// <param name="newObject">The new object to be added</param>
        /// <returns>The newly added object including its ID</returns>
        Task<T?> Add<T>(string? objectType, T? newObject);

        /// <summary>
        /// Updates a record in NetSuite by its Type and ID and returns the updated object
        /// </summary>
        /// <typeparam name="T">A custom data model representing the expected return structure</typeparam>
        /// <param name="objectType">The type of object to be updated</param>
        /// <param name="objectID">The id of the record to be updated</param>
        /// <param name="recordObject">The updated object</param>
        /// <returns>The updated object</returns>
        Task<T?> Update<T>(string? objectType, int? objectID, T? updatedRecord);

        /// <summary>
        /// Removes a record from NetSuite by its Type and ID and returns a bool to indicate if successful
        /// </summary>
        /// <typeparam name="T">A custom data model representing the expected return structure</typeparam>
        /// <param name="objectType">The type of object to be returned</param>
        /// <param name="objectID">The id of the record</param>
        /// <returns>A bool to indicate success or failure</returns>
        Task<bool?> Delete<T>(string? objectType, int? objectID);
    }
}
