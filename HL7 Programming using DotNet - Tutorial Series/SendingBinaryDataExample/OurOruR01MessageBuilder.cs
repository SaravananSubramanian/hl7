using System;
using System.Globalization;
using System.IO;
using CommonUtils;
using NHapi.Model.V23.Datatype;
using NHapi.Model.V23.Message;

namespace SendingBinaryDataExample
{
    internal class OurOruR01MessageBuilder
    {
        private ORU_R01 _oruR01Message;
        private readonly OurBase64Helper _ourBase64Helper = new OurBase64Helper();
        private readonly string _pdfFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test Files", "Sample Pathology Lab Report.pdf");
        
        /*You can pass in a domain or data transfer object as a parameter
        when integrating with data from your application here
        I will leave that to you to explore on your own
        Using fictional data here for illustration*/

        public ORU_R01 Build()
        {
            var currentDateTimeString = GetCurrentTimeStamp();
            _oruR01Message = new ORU_R01();
            
            CreateMshSegment(currentDateTimeString);
            CreatePidSegment();
            CreatePv1Segment();
            CreateObrSegment();
            CreateObxSegment();
            return _oruR01Message;
        }

        private void CreateMshSegment(string currentDateTimeString) 
        {
            var mshSegment = _oruR01Message.MSH;
            mshSegment.FieldSeparator.Value = "|";
            mshSegment.EncodingCharacters.Value = "^~\\&";
            mshSegment.SendingApplication.NamespaceID.Value = "Our System";
            mshSegment.SendingFacility.NamespaceID.Value = "Our Facility";
            mshSegment.ReceivingApplication.NamespaceID.Value = "Their Remote System";
            mshSegment.ReceivingFacility.NamespaceID.Value = "Their Remote Facility";
            mshSegment.DateTimeOfMessage.TimeOfAnEvent.Value = currentDateTimeString;
            mshSegment.MessageControlID.Value = GetSequenceNumber();
            mshSegment.MessageType.MessageType.Value = "ORU";
            mshSegment.MessageType.TriggerEvent.Value = "R01";
            mshSegment.VersionID.Value = "2.3";
            mshSegment.ProcessingID.ProcessingID.Value = "P";
        }

        private void CreatePidSegment() 
        {
            var pidSegment = _oruR01Message.GetRESPONSE().PATIENT.PID;

            var patientName = pidSegment.GetPatientName(0);
            patientName.FamilyName.Value = "Mouse";
            patientName.GivenName.Value = "Mickey";
            pidSegment.SetIDPatientID.Value = "378785433211";
            var patientAddress = pidSegment.GetPatientAddress(0);
            patientAddress.StreetAddress.Value = "123 Main Street";
            patientAddress.City.Value = "Lake Buena Vista";
            patientAddress.StateOrProvince.Value = "FL";
            patientAddress.Country.Value = "USA";
        }

        private void CreatePv1Segment() 
        {
            var patientInformation = _oruR01Message.GetRESPONSE().PATIENT;
            var visitInformation = patientInformation.VISIT;
            var pv1Segment = visitInformation.PV1;
            pv1Segment.PatientClass.Value = "O"; // to represent an 'Outpatient'
            var assignedPatientLocation = pv1Segment.AssignedPatientLocation;
            assignedPatientLocation.Facility.NamespaceID.Value = "Some Treatment Facility";
            assignedPatientLocation.PointOfCare.Value = "Some Point of Care";
            pv1Segment.AdmissionType.Value = "ALERT";
            var referringDoctor = pv1Segment.GetReferringDoctor(0);
            referringDoctor.IDNumber.Value = "99999999";
            referringDoctor.FamilyName.Value = "Smith";
            referringDoctor.GivenName.Value = "Jack";
            referringDoctor.IdentifierTypeCode.Value = "456789";
            pv1Segment.AdmitDateTime.TimeOfAnEvent.Value = GetCurrentTimeStamp();
        }

        private void CreateObrSegment()
        {
            var ourOrderObservation = _oruR01Message.GetRESPONSE().GetORDER_OBSERVATION();
            var obrSegment = ourOrderObservation.OBR;
            obrSegment.FillerOrderNumber.UniversalID.Value = "123456";
            obrSegment.UniversalServiceIdentifier.Text.Value = "Document";
            obrSegment.ObservationEndDateTime.TimeOfAnEvent.SetLongDate(DateTime.Now);
            obrSegment.ResultStatus.Value = "F";
        }

        private void CreateObxSegment()
        {
            var ourOrderObservation = _oruR01Message.GetRESPONSE().GetORDER_OBSERVATION();
            var observationSegment = ourOrderObservation.GetOBSERVATION(0);
            var obxSegment = observationSegment.OBX;

            obxSegment.SetIDOBX.Value = "0";
            //see HL7 table for list of permitted values here. We will use "Encapsulated Data" here
            obxSegment.ValueType.Value = "ED";
            obxSegment.ObservationIdentifier.Identifier.Value = "Report";

            //"Varies" is a NHAPI class to handle data where the appropriate 
            //data type is not known until run-time (e.g. OBX-5)
            var varies = obxSegment.GetObservationValue(0);
            var encapsulatedData = new ED(_oruR01Message, "PDF Report Content");

            encapsulatedData.SourceApplication.NamespaceID.Value = "Our .NET Application";
            encapsulatedData.TypeOfData.Value = "AP"; //see HL7 table 0191: Type of referenced data
            encapsulatedData.DataSubtype.Value = "PDF";
            encapsulatedData.Encoding.Value = "Base64";

            var base64EncodedStringOfPdfReport = _ourBase64Helper.ConvertToBase64String(new FileInfo(_pdfFilePath));
            encapsulatedData.Data.Value = base64EncodedStringOfPdfReport;
            varies.Data = encapsulatedData;
        }

        private static string GetCurrentTimeStamp()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss",CultureInfo.InvariantCulture);
        }

        private static string GetSequenceNumber()
        {
            const string facilityNumberPrefix = "1234"; // some arbitrary prefix for the facility
            return facilityNumberPrefix + GetCurrentTimeStamp();
        }
    }
}