using System;
using NHapi.Base;
using NHapi.Base.Log;
using NHapi.Base.Model;
using NHapi.Base.Parser;
using NHapi.Model.V22.Datatype;

namespace NHapi.Model.CustomZSegments.Segment
{	
	[Serializable]
	public sealed class ZPV : AbstractSegment  
	{  
		public ZPV(IGroup parent,IModelClassFactory factory) : base(parent, factory)
		{
			var message = Message;
			try
			{
				add(typeof(ST), true, 1, 13, new object[]{message}, "Custom Notes");
			    add(typeof(ST), true, 1, 13, new object[] { message }, "Custom Description");
            }
			catch (HL7Exception he)
			{
				HapiLogFactory.GetHapiLog(GetType()).Error("Unable to instantiate segment:" + GetType().Name, he);
			}
		}
		
		public ST CustomNotes
		{
			get
			{
				ST customNotes;
				try
				{
					var fieldData = GetField(1, 0);
					customNotes = (ST) fieldData;
				}
				catch (Exception ex)
				{
				    const string errorMessage = "Unexpected error occured while obtaining Custom Notes field value.";
				    HapiLogFactory.GetHapiLog(GetType()).Error(errorMessage, ex);
					throw new Exception(errorMessage, ex);
				}
				return customNotes;
			}
		}

		public ST CustomDescription
        {
			get
			{
				ST customDescription;
				try
				{
					var fieldData = GetField(2, 0);
				    customDescription = (ST)fieldData;
				}
				catch (Exception ex)
				{
				    var errorMessage = "Unexpected error occured while obtaining Custom Description field value.";
                    HapiLogFactory.GetHapiLog(GetType()).Error(errorMessage, ex);
					throw new Exception(errorMessage, ex);
				}
				return customDescription;
			}
		}
		
	}
}
