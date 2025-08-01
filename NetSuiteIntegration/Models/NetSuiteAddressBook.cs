﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static NetSuiteIntegration.Models.SharedEnum;

namespace NetSuiteIntegration.Models
{
    public class NetSuiteAddressBook
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
        public NetSuiteAddress? AddressBookAddress { get; set; }
        [JsonPropertyName("addressbookaddress_text")]
        public string? AddressBookAddressText { get; set; }
        public string? AddressID { get; set; }
        public bool? DefaultBilling { get; set; }
        public bool? DefaultShipping { get; set; }
        [Key]
        public int? ID { get; set; }
        [JsonIgnore]
        public int? InternalID { get; set; }
        public bool? IsResidential { get; set; }
        public string? Label { get; set; }

        //Extra Fields
        [JsonIgnore]
        public RecordActionType? RecordActionType { get; set; }
        [JsonIgnore]
        public NetSuiteAddress? Address { get; set; }
    }
}
