Please see GitHub page (https://github.com/duaneedwards/nHapi) for NHAPI tools for more additional information

Requirements:

You need to update the project's app.config <configSections>
element to include the 'Hl7PackageCollection' configuration section

 < configSections >
   < section name = "Hl7PackageCollection" type = "NHapi.Base.Model.Configuration.HL7PackageConfigurationSection, NHapi.Base" />
  </ configSections >
   
 And then you need to add a configuration that details the namespace and
 custom version name so that the PipeParser can find the custom definitions:

 <Hl7PackageCollection>
     <HL7Package name = "Replace with your custom model project namespace here" version="Replace with your version information"/>
 </Hl7PackageCollection>