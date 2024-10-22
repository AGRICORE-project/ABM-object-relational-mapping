using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AGRICORE_ABM_object_relational_mapping.Helpers
{
    /// <summary>
    /// Provides helper methods for error handling.
    /// </summary>
    public static class ErrorHelper
    {
        /// <summary>
        /// Retrieves a concatenated string of error descriptions from the model state.
        /// </summary>
        /// <param name="modelState">The model state containing validation errors.</param>
        /// <returns>A string containing all error messages concatenated by a semicolon.</returns>
        public static string GetErrorDescription (ModelStateDictionary modelState)
        {
            var errorList = modelState.Values.SelectMany(m => m.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return string.Join("; ", errorList);
        }
    }
}
