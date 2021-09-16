`timescale 1ns / 1ps
//////////////////////////////////////////////////////////////////////////////////
// Company:
// Engineer:
//
// Create Date: 2019/03/16 13:40:34
// Design Name:
// Module Name: uart_top
// Project Name:
// Target Devices:
// Tool Versions:
// Description:
//
// Dependencies:
//
// Revision:
// Revision 0.01 - File Created
// Additional Comments:
//
//////////////////////////////////////////////////////////////////////////////////


module uart_top
  #(
    parameter CLKS_PER_BIT = 87 // CLKS_PER_BIT = (Frequency of i_Clock)/(Frequency of UART)
  )
(
  input i_clk,

  // TX
  input i_tx_start,
  input [7:0] i_tx_byte,
  output o_tx_active, // Active high
  output o_tx,
  output o_tx_done, // A pulse indicate that TX is done

  // RX
  input i_rx,
  output [7:0] o_rx_byte,
  output o_rx_done // Pulse

  );
  uart_tx
  #(.CLKS_PER_BIT(CLKS_PER_BIT))
  serial_tx(
    .i_Clock(i_clk),
    .i_Tx_DV(i_tx_start),
    .i_Tx_Byte(i_tx_byte),
    .o_Tx_Active(o_tx_active),
    .o_Tx_Serial(o_tx),
    .o_Tx_Done(o_tx_done)
    );

  uart_rx
  #(.CLKS_PER_BIT(CLKS_PER_BIT))
  serial_rx(
    .i_Clock(i_clk),
    .i_Rx_Serial(i_rx),
    .o_Rx_DV(o_rx_done),
    .o_Rx_Byte(o_rx_byte)
    );
endmodule
