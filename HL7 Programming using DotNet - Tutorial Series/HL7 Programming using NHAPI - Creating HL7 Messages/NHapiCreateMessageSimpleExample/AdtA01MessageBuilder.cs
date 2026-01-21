using System.Globalization;
using NHapi.Model.V23.Message;

namespace Com.SaravananSubramanian.Nhapi.CreateMessages;

/// <summary>
/// Builder class for constructing ADT A01 (Admit/Visit Notification) messages.
/// Uses the Builder pattern to construct complex HL7 messages step by step.
/// </summary>
/// <remarks>
/// An ADT A01 message is sent when a patient is admitted as an inpatient or
/// registered as an outpatient. It typically contains:
/// - MSH: Message Header (routing and identification)
/// - EVN: Event Type (trigger event information)
/// - PID: Patient Identification (demographics)
/// - PV1: Patient Visit (visit/encounter information)
///
/// This example uses HL7 v2.3 message structures. For other versions,
/// change the using statement to the appropriate namespace (e.g., NHapi.Model.V24.Message).
/// </remarks>
public class AdtA01MessageBuilder
{
    private ADT_A01? _adtMessage;

    /// <summary>
    /// Builds a complete ADT A01 message with sample patient data.
    /// </summary>
    /// <returns>A fully populated ADT A01 message</returns>
    /// <remarks>
    /// In a real integration, you would pass in a domain object or DTO
    /// containing the actual patient and visit data from your system.
    /// </remarks>
    public ADT_A01 Build()
    {
        var timestamp = GetCurrentTimestamp();
        _adtMessage = new ADT_A01();

        // Build each segment in the order they appear in the message
        BuildMshSegment(timestamp);
        BuildEvnSegment(timestamp);
        BuildPidSegment();
        BuildPv1Segment();

        return _adtMessage;
    }

    /// <summary>
    /// Constructs the MSH (Message Header) segment.
    /// The MSH segment defines the message structure, encoding, and routing information.
    /// </summary>
    private void BuildMshSegment(string timestamp)
    {
        var msh = _adtMessage!.MSH;

        // Encoding characters - these define how the message is parsed
        // | = Field separator
        // ^ = Component separator
        // ~ = Repetition separator
        // \ = Escape character
        // & = Sub-component separator
        msh.FieldSeparator.Value = "|";
        msh.EncodingCharacters.Value = "^~\\&";

        // Sending and receiving application/facility identify the systems involved
        msh.SendingApplication.NamespaceID.Value = "Our System";
        msh.SendingFacility.NamespaceID.Value = "Our Facility";
        msh.ReceivingApplication.NamespaceID.Value = "Their Remote System";
        msh.ReceivingFacility.NamespaceID.Value = "Their Remote Facility";

        // Timestamp when the message was created
        msh.DateTimeOfMessage.TimeOfAnEvent.Value = timestamp;

        // Message Control ID - must be unique for tracking and acknowledgment matching
        msh.MessageControlID.Value = GenerateMessageControlId();

        // Message type identifies this as an ADT^A01 message
        msh.MessageType.MessageType.Value = "ADT";
        msh.MessageType.TriggerEvent.Value = "A01";

        // Version ID indicates the HL7 version (affects message structure)
        msh.VersionID.Value = "2.3";

        // Processing ID: P = Production, D = Debugging, T = Training
        msh.ProcessingID.ProcessingID.Value = "P";
    }

    /// <summary>
    /// Constructs the EVN (Event Type) segment.
    /// The EVN segment provides information about the triggering event.
    /// </summary>
    private void BuildEvnSegment(string timestamp)
    {
        var evn = _adtMessage!.EVN;

        // Event type code should match the trigger event in MSH-9
        evn.EventTypeCode.Value = "A01";

        // Recorded date/time - when this event was recorded in the system
        evn.RecordedDateTime.TimeOfAnEvent.Value = timestamp;

        // Additional EVN fields that can be populated:
        // evn.DateTimePlannedEvent.TimeOfAnEvent.Value = timestamp;
        // evn.EventReasonCode.Value = "01"; // Reason for the admit
        // evn.OperatorID.IDNumber.Value = "USER123";
    }

    /// <summary>
    /// Constructs the PID (Patient Identification) segment.
    /// The PID segment contains patient demographic information.
    /// </summary>
    private void BuildPidSegment()
    {
        var pid = _adtMessage!.PID;

        // Set ID - sequence number for multiple PID segments (usually "1")
        pid.SetIDPatientID.Value = "1";

        // Patient name - uses XPN (Extended Person Name) data type
        // Supports multiple repetitions for aliases, maiden names, etc.
        var patientName = pid.GetPatientName(0);
        patientName.FamilyName.Value = "Mouse";
        patientName.GivenName.Value = "Mickey";
        // Additional name components:
        // patientName.MiddleInitialOrName.Value = "M";
        // patientName.SuffixEgJRorIII.Value = "Jr";
        // patientName.NameTypeCode.Value = "L"; // Legal name

        // Patient address - uses XAD (Extended Address) data type
        var patientAddress = pid.GetPatientAddress(0);
        patientAddress.StreetAddress.Value = "123 Main Street";
        patientAddress.City.Value = "Lake Buena Vista";
        patientAddress.StateOrProvince.Value = "FL";
        patientAddress.ZipOrPostalCode.Value = "32830";
        patientAddress.Country.Value = "USA";

        // Additional commonly used PID fields:
        // pid.DateOfBirth.TimeOfAnEvent.Value = "19281118";
        // pid.Sex.Value = "M";
        // pid.PatientAccountNumber.ID.Value = "ACC123456";
        // pid.SSNNumberPatient.Value = "123-45-6789";
    }

    /// <summary>
    /// Constructs the PV1 (Patient Visit) segment.
    /// The PV1 segment contains information about the patient visit/encounter.
    /// </summary>
    private void BuildPv1Segment()
    {
        var pv1 = _adtMessage!.PV1;

        // Patient class: I = Inpatient, O = Outpatient, E = Emergency, etc.
        pv1.PatientClass.Value = "O";

        // Assigned patient location - where the patient is or will be
        var location = pv1.AssignedPatientLocation;
        location.Facility.NamespaceID.Value = "Some Treatment Facility";
        location.PointOfCare.Value = "ER"; // e.g., ward, clinic code
        location.Room.Value = "101";
        location.Bed.Value = "A";

        // Admission type: E = Emergency, U = Urgent, R = Routine, etc.
        pv1.AdmissionType.Value = "E";

        // Referring doctor information - uses XCN (Extended Composite ID and Name)
        var referringDoctor = pv1.GetReferringDoctor(0);
        referringDoctor.IDNumber.Value = "99999999";
        referringDoctor.FamilyName.Value = "Smith";
        referringDoctor.GivenName.Value = "Jack";
        referringDoctor.IdentifierTypeCode.Value = "NPI"; // National Provider Identifier

        // Admit date/time
        pv1.AdmitDateTime.TimeOfAnEvent.Value = GetCurrentTimestamp();

        // Additional commonly used PV1 fields:
        // pv1.AttendingDoctor(0).IDNumber.Value = "12345";
        // pv1.HospitalService.Value = "MED"; // Medical service
        // pv1.VisitNumber.ID.Value = "V123456";
    }

    /// <summary>
    /// Gets the current timestamp in HL7 format (yyyyMMddHHmmss).
    /// </summary>
    private static string GetCurrentTimestamp()
    {
        return DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Generates a unique message control ID.
    /// The message control ID must be unique for proper ACK matching.
    /// </summary>
    /// <remarks>
    /// In production systems, consider using:
    /// - A database sequence
    /// - A distributed ID generator (e.g., Snowflake)
    /// - A combination of facility ID + timestamp + sequence
    /// </remarks>
    private static string GenerateMessageControlId()
    {
        // Simple implementation: facility prefix + timestamp
        // For production, ensure uniqueness across distributed systems
        const string facilityPrefix = "FAC001";
        return $"{facilityPrefix}{GetCurrentTimestamp()}{Random.Shared.Next(1000, 9999)}";
    }
}
