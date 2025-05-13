using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NetSuiteIntegration.Models
{
    [Keyless]
    public class NetSuiteSearchParameter
    {
        public Operator? Operand { get; set; }
        public string? FieldName { get; set; }
        public Operator? Operator { get; set; }
        public string? Value { get; set; }
        public bool? IncludeOpeningParenthesis { get; set; }
        public bool? IncludeClosingParenthesis { get; set; }
    }

    public enum Operator
    {
        [Display(Name = "True/False: EMPTY")]
        EMPTY,
        [Display(Name = "True/False: EMPTY_NOT")]
        EMPTY_NOT,
        [Display(Name = "Number/Duration: ANY_OF")]
        ANY_OF,
        [Display(Name = "Number/Duration: ANY_OF_NOT")]
        ANY_OF_NOT,
        [Display(Name = "Number/Duration: BETWEEN")]
        BETWEEN,
        [Display(Name = "Number/Duration: BETWEEN_NOT")]
        BETWEEN_NOT,
        [Display(Name = "Number/Duration: EQUAL")]
        EQUAL,
        [Display(Name = "Number/Duration: EQUAL_NOT")]
        EQUAL_NOT,
        [Display(Name = "Number/Duration: GREATER")]
        GREATER,
        [Display(Name = "Number/Duration: GREATER_NOT")]
        GREATER_NOT,
        [Display(Name = "Number/Duration: GREATER_OR_EQUAL")]
        GREATER_OR_EQUAL,
        [Display(Name = "Number/Duration: GREATER_OR_EQUAL_NOT")]
        GREATER_OR_EQUAL_NOT,
        [Display(Name = "Number/Duration: LESS")]
        LESS,
        [Display(Name = "Number/Duration: LESS_NOT")]
        LESS_NOT,
        [Display(Name = "Number/Duration: LESS_OR_EQUAL")]
        LESS_OR_EQUAL,
        [Display(Name = "Number/Duration: LESS_OR_EQUAL_NOT")]
        LESS_OR_EQUAL_NOT,
        [Display(Name = "Number/Duration: WITHIN")]
        WITHIN,
        [Display(Name = "Number/Duration: WITHIN_NOT")]
        WITHIN_NOT,
        [Display(Name = "Text: CONTAIN")]
        CONTAIN,
        [Display(Name = "Text: CONTAIN_NOT")]
        CONTAIN_NOT,
        [Display(Name = "Text: IS")]
        IS,
        [Display(Name = "Text: IS_NOT")]
        IS_NOT,
        [Display(Name = "Text: START_WITH")]
        START_WITH,
        [Display(Name = "Text: START_WITH_NOT")]
        START_WITH_NOT,
        [Display(Name = "Text: END_WITH")]
        END_WITH,
        [Display(Name = "Text: END_WITH_NOT")]
        END_WITH_NOT,
        [Display(Name = "Date/Time: AFTER")]
        AFTER,
        [Display(Name = "Date/Time: AFTER_NOT")]
        AFTER_NOT,
        [Display(Name = "Date/Time: BEFORE")]
        BEFORE,
        [Display(Name = "Date/Time: BEFORE_NOT")]
        BEFORE_NOT,
        [Display(Name = "Date/Time: ON")]
        ON,
        [Display(Name = "Date/Time: ON_NOT")]
        ON_NOT,
        [Display(Name = "Date/Time: ON_OR_AFTER")]
        ON_OR_AFTER,
        [Display(Name = "Date/Time: ON_OR_AFTER_NOT")]
        ON_OR_AFTER_NOT,
        [Display(Name = "Date/Time: ON_OR_BEFORE")]
        ON_OR_BEFORE,
        [Display(Name = "Date/Time: ON_OR_BEFORE_NOT")]
        ON_OR_BEFORE_NOT
    }

    public enum Operand
    {
        [Display(Name = "AND")]
        AND,
        [Display(Name = "OR")]
        OR
    }
}
