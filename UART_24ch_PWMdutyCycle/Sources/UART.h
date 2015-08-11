/*
 * UART.h
 *
 *  Created on: Jul 21, 2015
 *      Author: Beartooth
 */

#ifndef UART_H_
#define UART_H_

#include "PE_Types.h"
#include "PE_LDD.h"

typedef struct {
  LDD_TDeviceData *handle; /* LDD device handle */
  volatile bool isSent; /* this will be set to 1 once the block has been sent */
  uint8_t rxChar; /* single character buffer for receiving chars */
  uint8_t (*rxPutFct)(uint8_t); /* callback to put received character into buffer */
} UART_Desc;

extern bool haveReceivedPacket; // boolean to tell main that a packet has been received
uint8_t receivedBytes;


void UART_Init(void);
void UART_Echo(unsigned char *strIN, unsigned char *strOUT);
void UART_SendString(unsigned char *str);
void UART_SendChar(unsigned char ch);
void UART_Listen(uint8_t *command,uint8_t verbosity);
void UART_GetPacket(byte *packet);
void UART_SendPacket(byte *tx_packet);
void UART_ParseData(void);
void UART_SendData(void); // function to decide what to send and to send to the PC

#endif /* UART_H_ */
