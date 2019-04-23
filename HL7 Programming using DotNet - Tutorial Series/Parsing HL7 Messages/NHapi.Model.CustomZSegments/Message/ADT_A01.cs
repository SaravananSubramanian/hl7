using NHapi.Base;
using NHapi.Base.Log;
using NHapi.Base.Parser;
using NHapi.Model.CustomZSegments.Segment;

namespace NHapi.Model.CustomZSegments.Message
{
    public class ADT_A01 : NHapi.Model.V23.Message.ADT_A01 
	{
        public ADT_A01(IModelClassFactory factory) : base(factory){
		   Init(factory);
		}

        public ADT_A01() : base(new DefaultModelClassFactory()) { 
		   Init(new DefaultModelClassFactory());
		}
	
		private void Init(IModelClassFactory factory) {
		   try 
		   {
		      add(typeof(ZPV),true,false); //mark this segment as required
		   } 
		   catch(HL7Exception e)
		   {
		      HapiLogFactory.GetHapiLog(GetType()).Error("Error creating ADT_A01", e);
		   }
		}
		
		public virtual ZPV ZPV
        {
			get
			{
			    ZPV segmentData = null;
				try
				{
					segmentData = (ZPV) this.GetStructure("ZPV");
				}
				catch (HL7Exception e)
				{
				    const string errorMessage = "Unexpected error accessing ZPV segment data";
				    HapiLogFactory.GetHapiLog(this.GetType()).Error(errorMessage, e);
					throw new System.Exception(errorMessage, e);
				}
				return segmentData;
			}
		}	
		
	}
}
