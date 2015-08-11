/* ###################################################################
**     Filename    : main.c
**     Project     : UART_24ch_PWMdutyCycle
**     Processor   : MKL25Z128VLK4
**     Version     : Driver 01.01
**     Compiler    : GNU C Compiler
**     Date/Time   : 2015-07-21, 09:16, # CodeGen: 0
**     Abstract    :
**         Main module.
**         This module contains user's application code.
**     Settings    :
**     Contents    :
**         No public methods
**
** ###################################################################*/
/*!
** @file main.c
** @version 01.01
** @brief
**         Main module.
**         This module contains user's application code.
*/         
/*!
**  @addtogroup main_module main module documentation
**  @{
*/         
/* MODULE main */


/* Including needed modules to compile this module/procedure */
#include "Cpu.h"
#include "Events.h"
#include "DMA1.h"
#include "AS1.h"
#include "RxBuf.h"
#include "AD1.h"
#include "AdcLdd1.h"
/* Including shared modules, which are used for whole project */
#include "PE_Types.h"
#include "PE_Error.h"
#include "PE_Const.h"
#include "IO_Map.h"

/* User includes (#include below this line is not maintained by Processor Expert) */
#include "data.h"
#include "UART.h"

const uint16 PWM_RATIO_TX = 0x1DFFU;
const uint16 PWM_RATIO_RX = 0x2DFFU;
const uint16 PWM_RATIO_IDLE = 0x32FFU;
const uint16 PWM_RATIO_OFF = 0xFFFFU; 
const uint16 PWM_RATIO_100MA = 0x32FFU;
const uint16 PWM_RATIO_250MA = 0x2DFFU;
const uint16 PWM_RATIO_500MA = 0x27FFU;
const uint16 PWM_RATIO_750MA = 0x247FU;
const uint16 PWM_RATIO_1000MA = 0x225FU;
const uint16 PWM_RATIO_1500MA = 0x1DFFU;
const uint16 PWM_RATIO_2000MA = 0x1BFFU;
const uint16 PWM_RATIO_2500MA = 0x1A9FU;
const uint16 PWM_RATIO_3000MA = 0x1907U;


const uint16 ADC_COUNT_100MA = 0x04D0U;
const uint16 ADC_COUNT_250MA = 0x0B50U;
const uint16 ADC_COUNT_500MA = 0x1720U;
const uint16 ADC_COUNT_750MA = 0x2200U;
const uint16 ADC_COUNT_1000MA = 0x2DB0U;
const uint16 ADC_COUNT_1500MA = 0x4480U;
const uint16 ADC_COUNT_2000MA = 0x5AF0U;
const uint16 ADC_COUNT_2500MA = 0x71B0U;
const uint16 ADC_COUNT_3000MA = 0x8890U;

const uint16 ADC_COUNT_IDLE = 0x0B50U;
const uint16 ADC_COUNT_RX = 0x0B50U;
const uint16 ADC_COUNT_TX = 0x4480U;

uint8 status = ERR_OK;
uint16 voltageValues[AD1_CHANNEL_COUNT] = {0}; // voltageValues stores the ADC counts
uint8 adcReadStatus = ERR_OK;
/*lint -save  -e970 Disable MISRA rule (6.3) checking. */
int main(void)
/*lint -restore Enable MISRA rule (6.3) checking. */
{
  /* Write your local variable definition here */


  /*** Processor Expert internal initialization. DON'T REMOVE THIS CODE!!! ***/
  PE_low_level_init();
  /*** End of Processor Expert internal initialization.                    ***/

  /* Write your code here */
  /* Enable the onboard RGB LED 
   *  port B pin 18 is red,
   *  port B pin 19 is green,
   *  port D pin 1 is blue         */
	SIM_SCGC5 |=0x00001400U; // enable port B and D clock control
	PORTB_PCR18 &=0xFFF0F8FFU; // disable interrupt requests from port B pin 18
	PORTB_PCR19 &=0xFFF0F8FFU; // disable interrupt requests from port B pin 19
	PORTD_PCR1 &=0xFFF0F0FFU; // disable interrupt requests from port D pin 1
	PORTB_PCR18 |=0x00000140U; // set port B pin 18 to have Drive Strength Enable
	PORTB_PCR19 |=0x00000140U; // set port B pin 19 to have Drive Strength Enable
	PORTD_PCR1 |=0x00000140U; // set port D pin 1 to have Drive Strength Enable (extra current to drive LED)
	GPIOB_PDDR |=0x000C0000U; // set gpio port B pins 18 and 19 to output
	GPIOD_PDDR |=0x00000002U; // set gpio port D pin 1 to output
	GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
	GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
	GPIOD_PSOR = 0x00000002U; // set pin D1 blue to high, off (active low LEDs)
  AD1_Calibrate(1); // Calibrate ADC and wait until done.
  UART_Init(); // initialize the UART
	
  for(;;) {
	  if(haveReceivedPacket){
		  // light to yellow for RX
			GPIOB_PCOR = 0x00040000U; // set pin B18 red to low, on (active low LEDs)
			GPIOB_PCOR = 0x00080000U; // set pin B19 green to low, off (active low LEDs)
			GPIOD_PSOR = 0x00000002U; // set pin D1 blue to high, off (active low LEDs)
		  UART_ParseData();
		  // turn off after transmit
			GPIOB_PSOR = 0x00040000U; // set pin B18 red to high, off (active low LEDs)
			GPIOB_PSOR = 0x00080000U; // set pin B19 green to high, off (active low LEDs)
			GPIOD_PSOR = 0x00000002U; // set pin D1 blue to high, off (active low LEDs)
	  }
  }

  /*** Don't write any code pass this line, or it will be deleted during code generation. ***/
  /*** RTOS startup code. Macro PEX_RTOS_START is defined by the RTOS component. DON'T MODIFY THIS CODE!!! ***/
  #ifdef PEX_RTOS_START
    PEX_RTOS_START();                  /* Startup of the selected RTOS. Macro is defined by the RTOS component. */
  #endif
  /*** End of RTOS startup code.  ***/
  /*** Processor Expert end of main routine. DON'T MODIFY THIS CODE!!! ***/
  for(;;){}
  /*** Processor Expert end of main routine. DON'T WRITE CODE BELOW!!! ***/
} /*** End of main routine. DO NOT MODIFY THIS TEXT!!! ***/

/* END main */
/*!
** @}
*/
/*
** ###################################################################
**
**     This file was created by Processor Expert 10.3 [05.09]
**     for the Freescale Kinetis series of microcontrollers.
**
** ###################################################################
*/
