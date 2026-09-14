using IAX.IXApi.Modules.Workflow.Controls;
using IAX.IXApi.Modules.Workflow.Processes;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Modules.Workflow.Requests
{
public class WfRequestControl: WfMasterEntity<long>
    {
        public long ProcessId { get; set; }
        [ForeignKey(nameof(ProcessId))]
        public virtual WfProcess Process { get; set; } = null!;
        public byte ControlId { get; set; }
        [ForeignKey(nameof(ControlId))]
        public virtual WfControl Control { get; set; } = null!;
        public decimal Score { get; set; }
        public byte SortOrder { get; set; }
        public string? ValidationRules { get; set; }  // as  xml 
        public string? ExtendedProperties { get; set; }  // as  xml 
        public bool CanFilter { get; set; } = true;
        public bool CanGroup { get; set; } = true;
        public bool CanSort { get; set; } = true;
        public string? ReferenceType { get; set; }
        public string FieldRole { get; set; } = "Dimension";
        public string DataType { get; set; } = "String";
        public string DefaultAggregation { get; set; } = "NONE";



        /*
         ReferenceType	FieldRole	DefaultAggregation	DataType	CanFilter	CanSort	CanGroup
NULL	Detail	NULL	Table	0	0	1
NULL	Dimension	NONE	Date	1	1	1
NULL	Dimension	NONE	String	0	0	0
NULL	Dimension	NONE	String	1	1	0
NULL	Dimension	NONE	String	1	1	1
NULL	Dimension	NONE	Time	1	1	1
NULL	Measure	SUM	Decimal	1	1	0
NULL	Measure	SUM	Integer	1	1	0
Employee	Dimension	NONE	String	1	1	1
File	Dimension	NONE	String	0	0	0
Option	Dimension	NONE	String	1	1	1
Showroom	Dimension	NONE	String	1	1	1
Signature	Dimension	NONE	String	0	0	0
         
         */
    }
}

