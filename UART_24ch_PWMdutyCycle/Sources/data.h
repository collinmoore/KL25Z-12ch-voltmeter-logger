/*
 * data.h
 *
 *  Created on: Jul 30, 2015
 *      Author: Beartooth
 */

#ifndef DATA_H_
#define DATA_H_

extern uint16 voltageValues[AD1_CHANNEL_COUNT]; // stores all ADC counts (5 16-bit words)
extern uint8 status;
extern uint8 adcReadStatus;
extern uint8 dutyCycle;
extern uint16 pwmRatio;
extern uint32 dutyCycleCounter;
extern bool firstDutySet;

#endif /* DATA_H_ */

