using System;

namespace SampleWebApi.Models
{
    public class ProcessVariableDto
    {
        public Int64 PersonId { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string PersonEmailAddress { get; set; }
        public string OfficerEmailAddress { get; set; }
        public Int64 TenantId { get; set; }
        public Int64 InstitutionId { get; set; }
        public string Reinsurance { get; set; }
    }
}
