# Training Exercise SNMP: Eaton Blade UPS

## Description

* Create the initial version protocol for the device EATON Blade UPS.\
The connector should meet delivery standards.
  * Cleaned up code and comments
  * Solved all DIS validator remarks
  * Protocol Parameters are:
    1. fully specified (good name, description, subtext, ranges, units, decimals, discrete options, ...)
    2. alarming/trending taken into account + default thresholds foreseen
  * No errors are visible in the element log file

## Setup

* The provided SNMP MIB files of the EATON Blade UPS are in the Documentation folder and load them into the DIS MIB browser in Visual Studio.\
This tool should be used to construct the connector.
* Load the provided simulation file which is available in the Documentation folder using the QA Device Simulation Tool\
[How to: Running Simulations](https://docs.dataminer.services/dataminer/DataMiner_Tools/QADeviceSimulator/Running_simulations.html)

## Connector Layout

* General Parameters
  * System Description
    * OID: 1.3.6.1.2.1.1.1.0
  * System Up Time
    * OID: 1.3.6.1.2.1.1.3.0
    * Display format: [Days]d,[Hours]h,[Minutes]m,[Seconds]s
  * Manufacturer
    * OID: 1.3.6.1.4.1.534.1.1.1.0
  * Model
    * OID: 1.3.6.1.4.1.534.1.1.2.0

* Interface Table
  * Interface Table
    * OID: 1.3.6.1.2.1.2.2
  * COLUMN Index
    * OID: 1.3.6.1.2.1.2.2.1.1
  * COLUMN Type
    * OID: 1.3.6.1.2.1.2.2.1.3
  * COLUMN Speed
    * OID: 1.3.6.1.2.1.2.2.1.5
  * COLUMN Administration Status
    * OID: 1.3.6.1.2.1.2.2.1.7
  * COLUMN Interface Speed
    * Calculated

To get the correct interface speed (Displayed in Mbps) you must follow the following calculations:

* If the initial reported interface speed is less than the unsigned integer max value this speed can be used.
* If the initial reported interface speed is equal to the unsigned integer max value, the value must be retrieved from the extended interface table.

* Extended Interface Table
  * Extended Interface Table
    * OID: 1.3.6.1.2.1.31.1.1
  * COLUMN Extended Speed
    * OID: 1.3.6.1.2.1.31.1.1.1.15

* UPS Parameters
  * Input Frequency
    * OID: 1.3.6.1.4.1.534.1.3.1.0
  * Input Phases
    * OID: 1.3.6.1.4.1.534.1.3.3.0
  * Output Load
    * OID: 1.3.6.1.4.1.534.1.4.1.0
  * Output Frequency
    * OID: 1.3.6.1.4.1.534.1.4.2.0
  * Battery Capacity
    * OID: 1.3.6.1.4.1.534.1.2.4.0
