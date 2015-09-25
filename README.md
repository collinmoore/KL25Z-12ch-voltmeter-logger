KL25z 12 channel voltmeter. Refresh the data and log data as long as you want.

To use:
	Download the .zip folder and extract the files.
	The folder UART_**** has code for the KL25z.
		To use, open codeWarrior and import existing project
		Point codeWarrior to the folder and import, flash, and run debug or just reset the MCU 
		to run the code.
	The folder Battery charger tester gui has C# code to run the logger application.
		To use, open the .sln file with visual studio and run debug
		Then, you can use the generated .exe
		The variables for current multipliers, timeouts, and such are in DataStorage class
		and connectionManager class.
.
Channel 0: IN voltage: Port E20
Channel 1: USB IN voltage: Port E21
Channel 2: Battery voltage: Port E22
Channel 3: System voltage: Port E23
Channels 4-7 are displayed as currents, must use the current monitor module.
Channel 4: IN current: Port E30
Channel 5: USB current: Port B0
Channel 6: Battery current: Port B1
Channel 7: System current: Port B2