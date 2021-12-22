// Maximum table sizes.  These control the sizes of UI tables we
// build; they aren't necessarily the actual maximum values that the
// firmware actually supports.  We build some of our tables out to
// fixed maximum sizes so that we can pre-build parts of the UI before
// loading any of the device configuration data, so we have to choose
// maximum sizes that are likely to be above the actual firmware
// maxima for the foreseeable future.  It would be better in principle
// to get the values from the device, but the KL25Z is pretty limited
// on memory, so the practical limits for most items really can't ever
// go much higher than they are now.  That lets us simplify our UI
// initialization work by picking fixed limits in the UI that are high
// enough that the device will likely never be able to exceed them.
var MaxOutputs = 128;   // maximum number of feedback device outputs
var MaxButtons = 128;   // maximum number of buttons


// Accessible KL25Z GPIO ports.  These are the ports that can be
// assigned to configurable functions, such as LedWiz outputs, key
// inputs, and outboard chip connections.  The CPU nominally has 160
// GPIO lines (five ports of 32 pins each), but only the subset shown
// below are physically wired to external pins on the Freescale board.
//
// Notes:
//
// - PTD0 is exposed as an external pin, but it's also hardwired on the
//   Freescale board to the blue segment of the on-board RGB LED.  The
//   port *can* be used for an external device, but doing so will give
//   up control over the blue LED for status displays.  The blue LED in
//   this case will still turn on and off according to the electrical
//   state of the pin, since there's no way to physically disconnect
//   the LED in software.  (You'd have to physically remove one of the
//   SMD resistors from the board, which I wouldn't recommend!)
//
// - PTB18 and PTB19 are included among the assignable ports even though
//   they're not connected to external pins.  We include them because
//   they're hard-wired to the on-board red and green segments of the RGB
//   LED (respectively), which makes them usable as LedWiz/DOF outputs.
//   When these GPIOs are assigned as output ports, the LedWiz or DOF
//   can control the on-board LED just like any other device.  These
//   outputs can't be wired to external devices, since the ports aren't
//   connected to header pins, but DOF access to the RGB LED could be
//   useful for purposes like debugging and testing.
//
// - PTC1 is exposed as an external pin, and it's also wired internally
//   on the KL25Z to SDA_PTD5 (port PTD5 on the *other* CPU on the KL25Z
//   board, which is the CPU that runs the OpenSDA boot loader and
//   debugger firwmare).  The mbed platform documentation specifies that
//   this pin is reserved for the RTC (real-time clock) reference clock
//   input.  PTC1 is in fact the only GPIO pin that can selected in the
//   pin mux as the RTC clock input, and it's wired to the SDA CPU so
//   that the SDA CPU can generate the required reference signal.  Based
//   on the mbed documentation and technical forum discussons, I thought
//   that the SDA CPU unconditionally generates the reference signal,
//   and since PTC1 is hard-wired on the KL25Z to SDA_PTD5, that this
//   would make PTC1 unusable as a GPIO for any other purpose.  However,
//   after further investigation, I found that it's only the *mbed*
//   version of the SDA boot loader that programs the SDA CPU to output
//   the clock signal.  The OpenSDA firmware that Pinscape users should
//   all be using is the PEMicro version, which does NOT generate the
//   clock signal on SDA_PTD5 - it appears to just leave the pin in
//   high-Z state, so that hard-wired connection to SDA_PTD5 ends up
//   being an open circuit.  On the KL25Z side, the mbed platform will
//   assign PTC1 on the pin-mux to RTC input duty, but *only if you
//   ask it to*, which Pinscape never does.  I previously thought that
//   the mbed platform always sets up the RTC in its startup code, but
//   after inspecting the code, I see that it doesn't - the RTC setup
//   is up to the application, so PTC1 is left unassigned by default.
//
//   In other words, PTC1 is free for us to use after all!  Before the
//   June 2021 release of the config tool, PTC1 was disabled because of
//   my incorrect understanding that mbed reserved it unconditionally,
//   but now that I see that that's not true, I enabled it for regular
//   GPIO use.
//   
// A limited subset of pins are capable of being used as PWM outputs,
// ADC inputs, or either.  The PWM-capable pins have a "pwm" property in
// the list below.  Likewise, the ADC-capable pins have an "adc"
// property.
//
// The "pwm" property for PWM-capable pins is in the format
// "TPM.channel", giving the TPM device number and channel tied to 
// the pin.  E.g., "2.0" means TPM #2, channel 0.  The TPMs ("Timer/
// PWM Modules") are the on-board peripherals that actually generate
// the PWM signals.  The CPU doesn't itself generate the PWM signals;
// those come from the TPMs.  Each PWM-capable GPIO pin is wired to
// exactly one TPM channel through the CPU multiplexer (an electrical
// switching network within the CPU).  The multiplexer makes the TPM
// connection optional: a PWM-capable pin can be connected to its
// TPM channel, or it can be connected to something else, such as one
// of the CPU digital in/out registers.  But this configurability only
// goes so far: the MUX connection only allows a particular PWM pin to
// connect to a particular, pre-determined TPM channel.  The association
// between a given GPIO pin and its assigned TPM channel isn't
// configurable.  These fixed pin-to-TPM connections are what's
// shown in the "pwm" properties.  For example, PTA1 can be connected
// through the MUX to TPM2 channel 0.  There are a total of 10
// distinct TPM channels connected to exposed GPIO pins.  However,
// there are more than 10 PWM-capable pins because some exposed TPM
// channels are connected to more than one GPIO pin.  In such cases,
// *only one* of the associated pins may be MUXed as a PWM.  For
// example, TPM2.0 is wired to PTA1, PTB2, PTB18, and PTE22.  Since
// all four pins map to the same TPM channel, and the TPM channel is
// what generates the PWM signal for the pins, these four pins can't
// be controlled as independent PWM outs.  Only *one* of these pins
// may thus be used at any given time as a PWM out - the rest must be
// used for some other purpose.  For example, if we assign PTA1 as a
// PWM out, it means we lose the ability to assign PTB2, PTB18, and
// PTE22 as PWM outs.  This is why we can only have 10 concurrent PWM
// outputs with this chip, even though it appears superficially that
// there are 31 PWM-capable pins.
//
// The TPM associations are important for another reason.  Each TPM
// unit has multiple channels that can be set to their own separate
// duty cycles, but any given TPM unit can only be set to a single
// frequency that applies to all of its channels.  For certain
// functions, we need to be able to set a PWM output to a specific
// frequency.  For example, the IR transmitter uses PWM to generate
// its modulation signal, so it has to set the TPM unit connected
// to its PWM output to the correct modulation frequency.  Similarly,
// the TLC5940 driver uses a PWM channel to generate the grayscale
// clock signal that the TLC5940 chip requires, so it needs to be
// able to set its PWM pin to the correct gsclock frequency.  If
// both the IR transmitter and TLC5940 driver are being used in the
// same system, they must be on separate TPM units, because each one
// needs to set its own special frequency.  So it's not good enough
// to merely put these devices on separate PWM output channels -
// they actually need to go on separate units.  That's why encode
// the unit information in the 'tpm' property separately from the
// channel number.
//
// The ADC-capable pins include an "adc" property in the format
// "module.channel".  E.g., 0.3 means ADC0 channel 3.  There's only
// one ADC module on the KL25Z, so the module number is always 0, but
// we include it for the sake of documentation.  The wiring between
// pins and ADC channels works just like with the PWM channels, so
// only the pins marked with "adc" can be used as ADC inputs at all,
// and each one can only be run through its hardwired ADC channel.
// The ADC setup is simpler than the TPM, though, in that there are
// no duplicate pin assignments for any ADC channels - meaning that
// every ADC-capable pin can be assigned as an ADC concurrently,
// without having to worry about overcommitting module channels.
//
// It's important to understand that the "pwm" and "adc" associations
// in the list below merely *document* the pin-to-channel connections.
// The entries don't determine them or affect what's connected to what.
// The connections are physically hard-wired within the CPU and simply
// can't be changed.  Changing the property values below won't change
// the way the pins are wired; it will only make the entries in the
// table wrong.
//
var gpioPins = [
    { name: "PTA1",  pwm: "2.0", interrupt: true },
    { name: "PTA2",  pwm: "2.1", interrupt: true },
    { name: "PTA4",  pwm: "0.1", i2c: "1.SDA", interrupt: true },
    { name: "PTA5",  pwm: "0.2", i2c: "1.SCL", interrupt: true },
    { name: "PTA12", pwm: "1.0", interrupt: true },
    { name: "PTA13", pwm: "1.1", interrupt: true },
    { name: "PTA16", interrupt: true  },
    { name: "PTA17", interrupt: true  },
    { name: "PTB0",  pwm: "1.0", adc: "0.8" },
    { name: "PTB1",  pwm: "1.1", adc: "0.9" },
    { name: "PTB2",  pwm: "2.0", adc: "0.12" },
    { name: "PTB3",  pwm: "2.1", adc: "0.13" },
    { name: "PTB8"   },
    { name: "PTB9"   },
    { name: "PTB10"  },
    { name: "PTB11"  },
    { name: "PTB18", pwm: "2.0", onBoardLED: 1,
                     internal: true, internalName: "Red on-board LED", internalShortName: "Red LED",
                     warning: "This port is hard-wired on the KL25Z to the red on-board LED. "
                              + "Using this port will supersede the red LED's normal status "
                              + "display function." },
    { name: "PTB19", pwm: "2.1", onBoardLED: 2,
                     internal: true, internalName: "Green on-board LED", internalShortName: "Green LED",
                     warning: "This port is hard-wired on the KL25Z to the green on-board LED. "
                              + "Using this port will supersede the green LED's normal status "
                              + "display function." },
    { name: "PTC0",  adc: "0.14" },

    // NOTE: PTC1 is also uniquely mux'able as RTC_CLOCK_INPUT.
    // The Pinscape firmware doesn't use the RTC at all, so this
    // pin is free for GPIO use.  If we ever add a feature that
    // depends upon the RTC, we'll have to mark this pin as in
    // conflict when that feature is enabled.
    { name: "PTC1",  pwm: "0.0", adc: "0.15", idc: "1.SCL" },

    { name: "PTC2",  pwm: "0.1", adc: "0.11", idc: "1.SDA" },
    { name: "PTC3",  pwm: "0.2" },
    { name: "PTC4",  pwm: "0.3" },
    { name: "PTC5",  spi: "SCLK" },
    { name: "PTC6",  spi: "MOSI" },
    { name: "PTC7",  spi: "MISO" },
    { name: "PTC8",  pwm: "0.4", i2c: "0.SCL" },
    { name: "PTC9",  pwm: "0.5", i2c: "0.SDA" },
    { name: "PTC10", i2c: "1.SCL"  },
    { name: "PTC11", i2c: "1.SDA"  },
    { name: "PTC12"  },
    { name: "PTC13"  },
    { name: "PTC16"  },
    { name: "PTC17"  },
    { name: "PTD0",  pwm: "0.0", interrupt: true },
    { name: "PTD1",  pwm: "0.1", adc: "0.5b", spi: "SCLK", onBoardLED: 3, interrupt: true,
                     remarks: "Blue on-board LED",
                     warning: "This port is hard-wired on the KL25Z to the blue on-board LED. "
                              + "You can use this port for other purposes, but doing so will "
                              + "supersede the blue LED's normal status display function." },
    { name: "PTD2",  pwm: "0.2", spi: "MOSI", interrupt: true },
    { name: "PTD3",  pwm: "0.3", spi: "MISO", interrupt: true },
    { name: "PTD4",  pwm: "0.4", interrupt: true },
    { name: "PTD5",  pwm: "0.5", adc: "0.6b", interrupt: true },
    { name: "PTD6",  adc: "0.7b", interrupt: true },
    { name: "PTD7",  interrupt: true},
    { name: "PTE0",  i2c: "1.SDA" },
    { name: "PTE1",  i2c: "1.SCL" },
    { name: "PTE2"   },
    { name: "PTE3"   },
    { name: "PTE4"   },
    { name: "PTE5"   },
    { name: "PTE20", pwm: "1.0", adc: "0.0" },
    { name: "PTE21", pwm: "1.1", adc: "0.4a" },
    { name: "PTE22", pwm: "2.0", adc: "0.3" },
    { name: "PTE23", pwm: "2.1", adc: "0.7a" },
    { name: "PTE29", pwm: "0.2", adc: "0.4b" },
    { name: "PTE30", pwm: "0.3", adc: "0.23" },
    { name: "PTE31", pwm: "0.4" }
];

// KL25Z pin headers
var kl25z_headers = {
    "J1": {
        pins: [["PTC7", "PTC0", "PTC3", "PTC4",  "PTC5", "PTC6", "PTC10", "PTC11"],
               ["PTA1", "PTA2", "PTD4", "PTA12", "PTA4", "PTA5", "PTC8",  "PTC9"]],
        type: "pinheader",
        pin1: [26, 88],
        pinN: [14, 174]
    },
    "J2": {
        pins: [["PTC12", "PTC13", "PTC16", "PTC17", "PTA16", "PTA17", "PTE31", "NC",    "PTD6", "PTD7"],
               ["PTA13", "PTD5",  "PTD0",  "PTD2",  "PTD3",  "PTD1",  "GND",   "VREFH", "PTE0", "PTE1"]],
        type: "pinheader",
        pin1: [26, 194],
        pinN: [14, 306]
    },
    "J9": {
        pins: [["PTB8",    "PTB9", "PTB10", "PTB11", "PTE2",  "PTE3", "PTE4", "PTE5"],
               ["SDA_PTD", "P3V3", "RESET", "P3V3",  "USB5V", "GND",  "GND",  "VIN"]],
        type:   "pinheader",
        pin1: [238, 262],
        pinN: [250, 175]
    },
    "J10": {
        pins: [["PTE20", "PTE21", "PTE22", "PTE23", "PTE29", "PTE30"],
               ["PTB0",  "PTB1",  "PTB2",  "PTB3",  "PTC2",  "PTC1"]],
        type: "pinheader",
        pin1: [238, 150],
        pinN: [250, 88]
    },

    // fake header representing the internal LED connections
    "LED": {
        pins: [["PTB18", "PTB19"]],
        type: "pinheader",
        pin1: [163, 137],
        pinN: [163, 152]
    }
};

// TLC5940 pins
var tlc5940_pins = {
    pins: ["OUT1",  "OUT2", "OUT3", "OUT4",  "OUT5",  "OUT6", "OUT7", "OUT8", "OUT9",  "OUT10", "OUT11", "OUT12", "OUT13", "OUT14",
           "OUT15", "XERR", "SOUT", "GSCLK", "DCPRG", "IREF", "VCC",  "GND",  "BLANK", "XLAT",  "SCLK",  "SIN",   "VPRG",  "OUT0"],
    type: "dip",
    pin1: [73, 46],
    pinN: [197, 368]
};

// 74HC595 pins
var hc595_pins = {
    pins: ["OUT1",  "OUT2", "OUT3", "OUT4",  "OUT5",  "OUT6", "OUT7", "GND",
           "OUT7'", "RST",  "SCLK", "LATCH", "ENA",   "SIN",  "OUT0", "VCC"],
    type: "dip",
    pin1: [85, 99],
    pinN: [188, 337]
};

// TLC59116 pins
var tlc59116_pins = {
    pins: ["REXT", "A0",   "A1",    "A2",    "A3",  "OUT0",  "OUT1",  "OUT2",  "OUT3",  "GND", "OUT4",  "OUT5", "OUT6", "OUT7",
           "OUT8", "OUT9", "OUT10", "OUT11", "GND", "OUT12", "OUT13", "OUT14", "OUT15", "GND", "RESET", "SCL",  "SDA",  "VCC"],
    type: "dip",
    pin1: [73, 46],
    pinN: [197, 368]
};

// Expansion board - main board headers
var mainBoard_headers = {
    "JP12": {
        pins: [["PTC4", "PTC0"],
               ["PTC3", "PTA2"]],
        type: "pinheader",
        pin1: [217, 76],
        pinN: [226, 67],
        orientation: "horizontal"
    },

    "JP11": {
        pins: [["3.8", "3.9", "3.10", "3.11", "3.12", "3.13", "3.14", "+5V"],
               ["3.0", "3.1", "3.2",  "3.3",  "3.4",  "3.5",  "3.6",  "3.7"]],
        type: "pinheader",
        pin1: [91, 137],
        pinN: [82, 77]
    },

    "JP8": {
        pins: [["3.16", "3.17", "3.18", "3.19", "3.20", "3.21", "3.28", "3.29", "3.30"],
               ["3.22", "3.23", "3.24", "3.25", "3.26", "3.27", "3.31", "NC", "+5V"]],
        type: "pinheader",
        pin1: [327, 152],
        pinN: [318, 83]
    },

    "JP3": {
        pins: [["PTE23", "LED+"],   // JP3-3 = LED+ = +5V; JP3-1 = Cal Btn LED- = open collector switched by PTE23
               ["PTE29", "GND"]],   // JP3-2 = Cal Btn = direct connection to PTE29
        type: "pinheader",
        orientation: "horizontal",
        pin1: [239, 336],
        pinN: [231, 345]
    },
    
    "JP2": {
        pins: [["PTB0", "PTE21", "3.3V", "5V"],
               ["NC",   "PTE22", "GND",  "PTE20"]],
        type: "pinheader",
        pin1: [206, 336],
        pinN: [181, 345]
    },
    
    "JP1": {
        pins: [["PTC2", "PTB2", "PTE30", "PTE5", "PTE3", "PTB11", "PTB9", "PTC12", "PTC16", "PTA16", "PTE31", "PTD7", "NC"],
               ["PTB3", "PTB1", "PTC11", "PTE4", "PTE2", "PTB10", "PTB8", "PTC13", "PTC17", "PTA17", "PTD6",  "PTE1", "COMMON"]],
        type: "pinheader",
        pin1: [144, 337],
        pinN: [43, 346]
    },

    "JP9": {
        pins: [["PTC8", "3.15"]],
        type: "pinheader",
        pin1: [190, 71],
        pinN: [199, 71]
    }

    // JP6 - PWM OUT.  For reference only; none of these pins
    // can be assigned as inputs or outputs
    //"JP6": {
    //    pins: [["SOUT", "SCLK", "XLAT", "BLANK", "GSCLK"],
    //           ["GND",  "GND",  "GND",  "GND",   "NC"]],
    //    type: "pinheader",
    //    pin1: [320, 214],
    //    pinN: [328, 249]
    //},

    // JP5 - Chime Out.  For reference only; these pins aren't
    // assignable as inputs or outputs.
    //"JP5": {
    //    pins: [["SOUT", "SCLK", "LATCH", "ENA", "3.3V"],
    //           ["NC",   "GND",  "GND",   "GND", "GND"]],
    //    type: "pinheader",
    //    pin1: [319, 285],
    //    pinN: [328, 320]
    //},

    // JP4 - TV switches.  For reference only; these pins can't
    // be assigned as inputs or outputs.
    //"JP4": {
    //    pins: [["TV1",  "TV2",  "IR+"],      // JP4-5 = IR+ = 5V
    //           ["TV1",  "TV2",  "PTC9"]],    // JP4-6 = IR- = open collector switched by PTC9
    //    type: "pinheader",
    //    pin1: [275, 309],
    //    pinN: [258, 317]
    //},
};


// Pinscape AIO board headers
var aioBoard_headers = {
    "Expansion Port": {
        pins: [["PTC0", "PTC4"],
               ["PTA2", "PTC3"]],
        type: "pinheader",
        pin1: [176, 359],
        pinN: [183, 364],
        orientation: "horizontal"
    },

    "Flashers and Strobe": {
        pins: [["3.0", "3.1", "3.2", "3.3", "3.4", "3.5", "3.6", "3.7", "3.8", "3.9", "3.10", "3.11", "3.12", "3.13", "3.14", "3.15"]],
        type: "pinheader",
        pin1: [10, 231],
        pinN: [10, 345]
    },

    "Small LED": {
        pins: [["3.16", "3.17", "3.18", "3.19", "3.20", "3.21", "3.22", "3.23", "3.24", "3.25", "3.26", "3.27", "3.28", "3.29", "3.30", "3.31"]],
        type: "pinheader",
        pin1: [10, 101],
        pinN: [10, 217]
    },

    "Calibration": {
        pins: [["PTE23", "LED+", "GND", "PTE29"]],   
        type: "pinheader",
        pin1: [143, 371],
        pinN: [165, 371]
    },
    
    "Plunger": {
        pins: [["5V", "PTE20", "PTE21", "PTE22", "PTB0", "3.3V", "GND"]],
        type: "pinheader",
        pin1: [195, 371],
        pinN: [250, 371]
    },
    
    "Button Inputs 1-8": {
        pins: [["PTC2", "PTB3", "PTB2", "PTB1", "PTE30", "PTC11", "PTE5", "PTE4"]],
        type: "pinheader",
        pin1: [258, 371],
        pinN: [313, 371]
    },

   "Button Inputs 9-24": {
        pins: [["PTE3", "PTE2", "PTB11", "PTB10", "PTB9", "PTB8", "PTC12", "PTC13", "PTC16", "PTC17", "PTA16", "PTA17", "PTE31", "PTD6", "PTD7", "PTE1"]],
        type: "pinheader",
        pin1: [334, 354],
        pinN: [334, 234]
    },

    "Power Outputs 1-16": {
        pins: [["3.32", "3.33", "3.34", "3.35", "3.36", "3.37", "3.38", "3.39", "3.40", "3.41", "3.42", "3.43", "3.44", "3.45", "3.46", "3.47"]],
        type: "pinheader",
        pin1: [334, 222],
        pinN: [334, 101]
    },
    
    "Power Outputs 17-32": {
        pins: [["3.48", "3.49", "3.50", "3.51", "3.52", "3.53", "3.54", "3.55", "3.56", "3.57", "3.58", "3.59", "3.60", "3.61", "3.62", "3.63"]],
        type: "pinheader",
        pin1: [284, 46],
        pinN: [163, 46]
    },

    "Knocker": {
        pins: [["PTC8"]],
        type: "pinheader",
        pin1: [98, 46],
        pinN: [98, 46]
    },

    "Chime Outputs 1-8": {
        pins: [["4.0", "4.1", "4.2", "4.3", "4.4", "4.5", "4.6", "4.7"]],
        type: "pinheader",
        pin1: [89, 46],
        pinN: [34, 46]
    }
};

// Pinscape Lite board headers
var liteBoard_headers = {

    "Small LED": {
        pins: [["3.0", "3.1", "3.2", "3.3", "3.4", "3.5", "3.6", "3.7", "3.8", "3.9", "3.10", "3.11", "3.12", "3.13", "3.14", "3.15"]],
        type: "pinheader",
        pin1: [10, 134],
        pinN: [10, 324]
    },

    "Plunger": {
        pins: [["5V", "PTE20", "PTE21", "PTE22", "PTB0", "3.3V", "GND"]],
        type: "pinheader",
        pin1: [98, 365],
        pinN: [168, 365]
    },

    "Button Inputs 1-8": {
        pins: [["PTC2", "PTB3", "PTB2", "PTB1", "PTE30", "PTC11", "PTE5", "PTE4"]],
        type: "pinheader",
        pin1: [207, 365],
        pinN: [295, 365]
    },

    "Button Inputs 9-24": {
        pins: [["PTE3", "PTE2", "PTB11", "PTB10", "PTB9", "PTB8", "PTC12", "PTC13", "PTC16", "PTC17", "PTA16", "PTA17", "PTE31", "PTD6", "PTD7", "PTE1"]],
        type: "pinheader",
        pin1: [334, 328],
        pinN: [334, 136]
    },

    "Power Outputs": {
        pins: [["PTA2", "PTA13", "PTD2", "PTD3", "PTC8", "PTC9", "PTC0", "PTC3", "PTC4", "PTE23", "PTE29", "PTE0"]],
        type: "pinheader",
        pin1: [278, 48],
        pinN: [134, 48]
    },
};

// Arnoz RigMaster board headers
var rigMasterBoard_headers = {

    "Boutons": {
        pins: [["PTE30", "PTE29", "PTE23", "PTE22", "PTE21", "PTE20", "PTE5", "PTE4", "PTE3", "PTE2", "PTE1", "PTE0", "PTD7"]],
        type: "pinheader",
        pin1: [219, 325],
        pinN: [295, 325],
        orientation: "horizontal"
    },

    "Brain": {
        pins: [["PTD6", "PTD5", "PTD4", "PTD3", "PTD2", "PTC7", "PTC17", "PTC16", "GND", "5V", "PTC13", "PTC12"]],
        type: "pinheader",
        pin1: [14, 234],
        pinN: [14, 300]
    },

    "Plunger": {
        pins: [["PTE21", "GND", "PTB0", "3.3V"]],
        type: "pinheader",
        pin1: [335, 258],
        pinN: [335, 280]
    },

    "Extension": {
        pins: [["PTA13", "PTC5", "PTC6", "PTC10", "PTC11", "5V", "GND"]],
        type: "pinheader",
        pin1: [58, 325],
        pinN: [98, 325],
        orientation: "horizontal"
    },

    "Digital Outputs 1-4": {
        pins: [["PTB1", "PTB2", "PTB3", "PTB8"]],
        type: "pinheader",
        pin1: [68, 80],
        pinN: [84, 80],
        orientation: "horizontal"
    },
    "Digital Outputs 5-8": {
        pins: [["PTB9", "PTB10", "PTB11", "PTA17"]],
        type: "pinheader",
        pin1: [128, 80],
        pinN: [146, 80],
        orientation: "horizontal"
    },
    "PWM Outputs 9-12": {
        pins: [["PTA1", "PTA2", "PTA4", "PTA5"]],
        type: "pinheader",
        pin1: [188, 80],
        pinN: [204, 80],
        orientation: "horizontal"
    },
    "PWM Outputs 13-16": {
        pins: [["PTD0", "PTC4", "PTC8", "PTC9"]],
        type: "pinheader",
        pin1: [264, 80],
        pinN: [282, 80],
        orientation: "horizontal"
    },
};

// Arnoz KLShield board headers
var klShieldBoard_headers = {

    "BOUTON 1": {
        pins: [["GND", "PTE30", "PTE29", "PTE23", "PTE22", "PTE21", "PTE20", "PTE5", "PTE4", "PTE3", "PTE2", "GND"]],
        type: "pinheader",
        pin1: [13, 80],
        pinN: [13, 205],
    },

    "BOUTON 2": {
        pins: [["PTC16", "PTC17", "PTC7", "PTD2", "PTD3", "PTD4", "PTD5", "PTD6", "PTD7", "PTE0", "PTE1", "GND"]],
        type: "pinheader",
        pin1: [110, 80],
        pinN: [110, 215],
    },

    "OUT 1": {
        pins: [["GND", "PTB11", "PTB10", "PTB9", "PTB8", "PTB3", "PTB2", "PTB1", "PTA17", "PTA16", "PTA12", "GND"]],
        type: "pinheader",
        pin1: [330, 95],
        pinN: [330, 218],
    },

    "OUT 2": {
        pins: [["GND", "PTC0", "PTC1", "PTC2", "PTC3", "PTE31", "PTB0", "SDA_PTD", "PTD1", "RESET", "GND", "GND"]],
        type: "pinheader",
        pin1: [230, 85],
        pinN: [230, 218],
    },

    "PLUNGER": {
        pins: [["P3V3", "PTB0", "GND", "GND", "USB5V", "PTC13", "PTC12"]],
        type: "pinheader",
        pin1: [13, 230],
        pinN: [13, 300]
    },

    "EXTENSION": {
        pins: [["PTA13", "PTC5", "PTC6", "PTC10", "PTC11", "USB5V", "GND"]],
        type: "pinheader",
        pin1: [97, 347],
        pinN: [165, 347],
        orientation: "horizontal"
    },

    "PWM MOS": {
        pins: [["PTA1", "PTA2", "PTA4", "PTA5", "PTD0", "PTC4", "PTC8", "PTC9", "GND"]],
        type: "pinheader",
        pin1: [190, 347],
        pinN: [277, 347],
        orientation: "horizontal"
    },

    "VOLTS": {
        pins: [["VREFH", "VIN", "USB5V", "P3V3"]],
        type: "pinheader",
        pin1: [240, 200],
        pinN: [240, 220],
        orientation: "horizontal"
    },

};

// Arnoz Mollusk board
var molluskBoard_headers = {
    "OUT1-8": {
        pins: [["3.0", "3.1", "3.2", "3.3", "3.4", "3.5", "3.6", "3.7"]],
        type: "pinheader",
        pin1: [80, 120],
        pinN: [170, 120],
        orientation: "horizontal"
    },

    "OUT9-16": {
        pins: [["3.8", "3.9", "3.10", "3.11", "3.12", "3.13", "3.14", "3.15"]],
        type: "pinheader",
        pin1: [210, 120],
        pinN: [300, 120],
        orientation: "horizontal"
    }

    // for reference only - these pins aren't assignable
    //"DATA-in": {
    //    pins: [["GSCLK",  "SCLK", "SIN", "XLAT", "BLANK", "5V", "GND"],
    //    type: "pinheader",
    //    pin1: [318, 212],
    //    pinN: [327, 246],
    //    orientation: "horizontal"
    //},
    //"DATA-out": {
    //    pins: [["GSCLK",  "SCLK", "SOUT", "XLAT", "BLANK", "5V", "GND"],
    //    type: "pinheader",
    //    pin1: [318, 292],
    //    pinN: [327, 327],
    //    orientation: "horizontal"
    //}
};

// Pinscape expansion boards - Power Board
var powerBoard_headers = {
    "JP5": {
        pins: [["3.32", "3.33", "3.34", "3.35", "3.36", "3.37", "3.38", "3.39", "3.40", "3.41", "3.42", "3.43", "3.44", "3.45", "3.46", "3.47"]],
        type: "pinheader",
        pin1: [97, 49],
        pinN: [225, 49]
    },
    
    "JP6": {
        pins: [["3.48", "3.49", "3.50", "3.51", "3.52", "3.53", "3.54", "3.55", "3.56", "3.57", "3.58", "3.59", "3.60", "3.61", "3.62", "3.63"]],
        type: "pinheader",
        pin1: [97, 361],
        pinN: [225, 361]
    }

    // for reference only - these pins aren't assignable
    //"JP2": {
    //    pins: [["SIN",  "SCLK", "XLAT", "BLANK", "GSCLK"],
    //           ["GND",  "GND",  "GND",  "GND",   "GND"]],
    //    type: "pinheader",
    //    pin1: [318, 212],
    //    pinN: [327, 246]
    //},
    //"JP3": {
    //    pins: [["SOUT", "SCLK", "XLAT", "BLANK", "GSCLK"],
    //           ["GND",  "GND",  "GND",  "GND",   "GND"]],
    //    type: "pinheader",
    //    pin1: [318, 292],
    //    pinN: [327, 327]
    //}
};

var aio_powerBoard_headers = {
    "JP5": {
        pins: [["3.64", "3.65", "3.66", "3.67", "3.68", "3.69", "3.70", "3.71", "3.72", "3.73", "3.74", "3.75", "3.76", "3.77", "3.78", "3.79"]],
        type: "pinheader",
        pin1: [97, 49],
        pinN: [225, 49]
    },
    
    "JP6": {
        pins: [["3.80", "3.81", "3.82", "3.83", "3.84", "3.85", "3.86", "3.87", "3.88", "3.89", "3.90", "3.91", "3.92", "3.93", "3.94", "3.95"]],
        type: "pinheader",
        pin1: [97, 361],
        pinN: [225, 361]
    }
};

// future feature
var lite_powerBoard_headers = {
    "JP5": {
        pins: [["3.16", "3.17", "3.18", "3.19", "3.20", "3.21", "3.22", "3.23", "3.24", "3.25", "3.26", "3.27", "3.28", "3.29", "3.30", "3.31"]],
        type: "pinheader",
        pin1: [97, 49],
        pinN: [225, 49]
    },

    "JP6": {
        pins: [["3.32", "3.33", "3.34", "3.35", "3.36", "3.37", "3.38", "3.39", "3.40", "3.41", "3.42", "3.43", "3.44", "3.45", "3.46", "3.47"]],
        type: "pinheader",
        pin1: [97, 361],
        pinN: [225, 361]
    }
};

var chimeBoard_headers = {
    "JP9": {
        pins: [["4.0", "4.1", "4.2", "4.3", "4.4", "4.5", "4.6", "4.7"]],
        type: "pinheader",
        pin1: [113, 68],
        pinN: [174, 68]
    }

    // for reference only - these pins aren't assignable
    //"JP2": {
    //    pins: [["SOUT", "SCLK", "LATCH", "ENA", "3.3V"],
    //           ["NC",   "GND",  "GND",   "GND", "GND"]],
    //    type: "pinheader",
    //    pin1: [319, 224],
    //    pinN: [328, 259]
    //},
    //"JP3": {
    //    pins: [["SIN",  "SCLK", "LATCH", "ENA", "3.3V"],
    //           ["NC",   "GND",  "GND",   "GND", "GND"]],
    //    type: "pinheader",
    //    pin1: [319, 299],
    //    pinN: [328, 334]
    //}
};

var aio_chimeBoard_headers = {
    "JP9": {
        pins: [["4.8", "4.9", "4.10", "4.11", "4.12", "4.13", "4.14", "4.15"]],
        type: "pinheader",
        pin1: [113, 68],
        pinN: [174, 68]
    }
};

// future feature
var lite_chimeBoard_headers = {
    "JP9": {
        pins: [["4.0", "4.1", "4.2", "4.3", "4.4", "4.5", "4.6", "4.7"]],
        type: "pinheader",
        pin1: [113, 68],
        pinN: [174, 68]
    }
};

// build a hash of the pin table indexed by name
var gpioPinsByName = { };
for (var i = 0 ; i < gpioPins.length ; ++i)
    gpioPinsByName[gpioPins[i].name] = gpioPins[i];

$.each(kl25z_headers, function(k, v) {
    forEachPin(v, function(pin, n, x, y) {
        var g = gpioPinsByName[pin] || { };
        if (g)
            g.pin = { name: k + "-" + n, x: x, y: y };
    });
});

// get the jumper location for a given pin
function pinToJumper(pin)
{
    var g = gpioPinsByName[pin] || {};
    return g.internalShortName || (g.pin || {}).name;
}

// Extract the TPM unit from a PWM channel name.  Channel
// names are in the format "1.2", where 1 is the TPM unit
// number and 2 is the channel number.  We pull out just
// the TPM unit number ("1" in this case).
function pwmToTpm(pwm)
{
    return /(\d+)\.\d+/.test(pwm) ? RegExp.$1 : null;
}

// build a hash of PWM channels and the associated pins
var pwmChannels = { };
var tpmUnits = { };
for (var i = 0 ; i < gpioPins.length ; ++i)
{
    var g = gpioPins[i];
    if (g.pwm)
    {
        var t = pwmChannels[g.pwm];
        if (!t)
            pwmChannels[g.pwm] = t = {
                name: g.pwm,
                orList: function() { return serialComma($.map(this.pins, function(ele) { return ele.name; }), "or"); },
                pins: []
            };
        t.pins.push(g);

        var tpm = pwmToTpm(g.pwm);
        if (!(t = tpmUnits[tpm]))
        {
            tpmUnits[tpm] = t = {
                name: tpm,
                orList: function() { return serialComma($.map(this.pins, function(ele) { return ele.name; }), "or"); },
                pins: []
            };
        }
        t.pins.push(g);
    }
}

// Iterate over a pin header object.  Calls func(name, n, x, y), where
// 'name' is the pin name, 'n' is the pin number (starting at 1), and
// 'x' and 'y' give the spatial coordinates.
function forEachPin(header, func)
{
    ({ "dip": forEachDIPPin, "pinheader": forEachHeaderPin })[header.type](header, func);
}

// DIP IC chip pins are conventionally arranged clockwise around
// the chip, starting with pin 1 at the lower left:
//
//   n n-1 n-2 ... n/2+1
//   1 2   3   ... n/2
//
// Chips can of course be rotated on the board.  We allow rotations
// in 90 degree increments.  We infer the orientation from the
// positions of the first pin and diametrically opposed pins, given
// by properties pin1=[x,y] and pinN=[x,y].  The pin positions are
// given as image-relative coordinates of the centers of the
// respective pins.  Note that pinN isn't the highest numbered pin:
// it's actually the diametrically opposed pin, so the first pin
// in the second row at pin number n/2+1.
function forEachDIPPin(chip, func)
{
    // get the number of pins, and the number on each side (it's a DIP,
    // so there are exactly two rows of pins)
    var npins = chip.pins.length;
    var oneSidePins = npins/2;

    // get the pin 1 location (pin1=[x,y] position of center of pin 1)
    var x1 = chip.pin1[0];
    var y1 = chip.pin1[1];

    // get the last pin location (pinN=[x,y] position of center of last pin)
    var xN = chip.pinN[0];
    var yN = chip.pinN[1];

    // Infer the orientation.  If pin1 is at the lower left or upper right,
    // it's horizontal, otherwise it's vertical.
    var horizontal = ((x1 < xN && y1 > yN) || (x1 > xN && y1 < yN));

    // figure the iteration increments
    var pindx = 0, pindy = 0, rowdx = 0, rowdy = 0;
    if (horizontal)
    {
        if (oneSidePins > 1) pindx = (xN - x1)/(oneSidePins - 1);
        rowdy = yN - y1;
    }
    else
    {
        if (oneSidePins > 1) pindy = (yN - y1)/(oneSidePins - 1);
        rowdx = xN - x1;
    }

    // go up the first row, starting at pin 1
    var pins = chip.pins;
    var x = x1, y = y1;
    for (var pin = 0 ; pin < oneSidePins ; ++pin, x += pindx, y += pindy)
        func(pins[pin], pin, x, y);

    // go down the second row, reversing direction
    for (x += rowdx, y += rowdy, x -= pindx, y -= pindy ; pin < npins ; ++pin, x -= pindx, y -= pindy)
        func(pins[pin], pin, x, y);
}

// Pin headers are arranged into rows and columns, where the
// row is the long dimension.  E.g., a 2x10 header has 2 rows
// of 10 pins, and a 3x15 has 3 rows of 15 pins.
//
// We use a zig-zag numbering scheme.  Pin 1 is always at
// one corner.  Pin 2 is the next pin in the same column.
// Continue numbering up the column until reaching the last
// pin, then go back to the next pin in the starting row.
// Repeat until done.
//
// This conventional is pretty universal, but the headers
// can of course be rotated, and they can also be mirrored.
// We allow for four rotations, at right angles.
//
// header.pins = [[row1], [row2], [row3]...] - pin name strings
// header.pin1 = [x,y] - center coordinates of pin 1, relative to image
// header.pinN = [x,y] - center coordinates of LAST pin, relative to image
//
// Important: the pin1 and pinN positions are the coordinates
// of the centers of the respective pins, not of the header
// plastic or shroud.  Also, note that the property name pinN
// is literal - the N isn't meant to be replaced with a number,
// it's just a literal N.
//
// For reference, here are the standard numbering layouts in
// the cardinal rotations.  In most cases, we can work out the
// orientation on our own, by noting which dimension (x or y)
// is longer - the long dimension is the row.  The exception
// is when the pin array is square, such as a 2x2.  In this
// case, we need to know whether it's a horizontal or vertical
// orientation.  Set header.orientation to "horizontal" or
// "vertical" in this case to tell us which it is.  (You can
// set that in all cases, and it'll override our inference if
// set, but it's easier to omit it when not necessary.)
//
//            0 deg        90 deg      180 deg     270 deg
//
//                           6 5                     1 2
//            2 4 6          4 3        5 3 1        3 4
//  normal    1 3 5          2 1        6 4 2        5 6
//
//                           2 1                     5 6
//            6 4 2          4 3        1 3 5        3 4
//  mirror    5 3 1          6 5        2 5 6        1 2
//
function forEachHeaderPin(header, func)
{
    // get the number of pin rows in this header, and number of pins in each row
    var nrows = header.pins.length;
    var npins = header.pins[0].length;

    // get the pin 1 location (header.pin1 = [x,y])
    var x1 = header.pin1[0];
    var y1 = header.pin1[1];

    // get the position of the last pin (pinN)
    var xN = header.pinN[0];
    var yN = header.pinN[1];

    // figure the orientation - user the header.orientation setting if provided,
    // of infer it from which dimension is longer
    var wid = Math.abs(x1 - xN);
    var ht = Math.abs(y1 - yN);
    var orientation = header.orientation || (wid > ht ? "horizontal" : "vertical");

    // figure the column (pin) and row increments for the iteration
    var rowdx = 0, rowdy = 0, pindx = 0, pindy = 0;
    if (orientation == "horizontal")
    {
        // horizontal
        if (npins > 1) pindx = (xN - x1)/(npins - 1);
        if (nrows > 1) rowdy = (yN - y1)/(nrows - 1);
    }
    else
    {
        // vertical
        if (npins > 1) pindy = (yN - y1)/(npins - 1);
        if (nrows > 1) rowdx = (xN - x1)/(nrows - 1);
    }

    // start at pin 1
    var xrow = x1, yrow = y1;

    // process each row
    for (var row = 0 ; row < nrows ; ++row, xrow += rowdx, yrow += rowdy)
    {
        // start at the first pin of the row
        var pinno = row+1;
        var x = xrow, y = yrow;
        var pinrow = header.pins[row];
        
        // process each pin in the row
        for (var pin = 0 ; pin < npins ; ++pin, x += pindx, y += pindy, pinno += nrows)
            func(pinrow[pin], pinno, x, y);
    }
}

// Factory configuration for the stand-alone KL25Z (no expansion boards)
var standaloneFactoryConfig = {
    expansionBoards: {
        version: "0",
        ext0: "0",
        ext1: "0",
        ext2: "0",
        ext3: "0"
    },
    calButtonPins: {
        enabled: 0x03,
        button: "PTE29",
        led: "PTE23"
    },
    TVon: {
        statusPin: "NC",
        latchPin: "NC",
        relayPin: "NC",
        delay: 0
    },
    TLC5940: {
        nchips: 0,
        SIN: "PTC6",
        SCLK: "PTC5",
        XLAT: "PTC10",
        BLANK: "PTC7",
        GSCLK: "PTA1"
    },
    HC595: {
        nchips: 0,
        SIN: "PTA5",
        SCLK: "PTA4",
        LATCH: "PTA12",
        ENA: "PTD4"
    },
    TLC59116: {
        chipMask: 0,
        SDA: "NC",
        SCL: "NC",
        RESET: "NC"
    },
    ZBLaunchBall: {
        port: 0,
        keytype: 2,             // keyboard key
        keycode: 0x28,          // Enter key
        pushDistance: 63        // .063" ~ 1/16"
    },
    buttons: {
        // use the defaults from earlier versions, where everything was mapped to joystick buttons
        1:  { pin: "PTC2",  keytype: 1, keycode: 1, flags: 0 },
        2:  { pin: "PTB3",  keytype: 1, keycode: 2, flags: 0 },
        3:  { pin: "PTB2",  keytype: 1, keycode: 3, flags: 0 },
        4:  { pin: "PTB1",  keytype: 1, keycode: 4, flags: 0 },
        5:  { pin: "PTE30", keytype: 1, keycode: 5, flags: 0 },
        6:  { pin: "PTE22", keytype: 1, keycode: 6, flags: 0 },
        7:  { pin: "PTE5",  keytype: 1, keycode: 7, flags: 0 },
        8:  { pin: "PTE4",  keytype: 1, keycode: 8, flags: 0 },
        9:  { pin: "PTE3",  keytype: 1, keycode: 9, flags: 0 },
        10: { pin: "PTE2",  keytype: 1, keycode: 10, flags: 0 },
        11: { pin: "PTB11", keytype: 1, keycode: 11, flags: 0 },
        12: { pin: "PTB10", keytype: 1, keycode: 12, flags: 0 },
        13: { pin: "PTB9",  keytype: 1, keycode: 13, flags: 0 },
        14: { pin: "PTB8",  keytype: 1, keycode: 14, flags: 0 },
        15: { pin: "PTC12", keytype: 1, keycode: 15, flags: 0 },
        16: { pin: "PTC13", keytype: 1, keycode: 16, flags: 0 },
        17: { pin: "PTC16", keytype: 1, keycode: 17, flags: 0 },
        18: { pin: "PTC17", keytype: 1, keycode: 18, flags: 0 },
        19: { pin: "PTA16", keytype: 1, keycode: 19, flags: 0 },
        20: { pin: "PTA17", keytype: 1, keycode: 20, flags: 0 },
        21: { pin: "PTE31", keytype: 1, keycode: 21, flags: 0 },
        22: { pin: "PTD6",  keytype: 1, keycode: 22, flags: 0 },
        23: { pin: "PTD7",  keytype: 1, keycode: 23, flags: 0 },
        24: { pin: "PTE1",  keytype: 1, keycode: 24, flags: 0 }
    },
    outputs: {
        1: { port: { type: 1, pin: "PTA1" }, flags: 0x00 },     // port 1  = PTA1 (PWM)
        2: { port: { type: 1, pin: "PTA2" }, flags: 0x00 },     // port 2  = PTA2 (PWM)
        3: { port: { type: 1, pin: "PTD4" }, flags: 0x00 },     // port 3  = PTD4 (PWM)
        4: { port: { type: 1, pin: "PTA12" }, flags: 0x00 },    // port 4  = PTA12 (PWM)
        5: { port: { type: 1, pin: "PTA4" }, flags: 0x00 },     // port 5  = PTA4 (PWM)
        6: { port: { type: 1, pin: "PTA5" }, flags: 0x00 },     // port 6  = PTA5 (PWM)
        7: { port: { type: 1, pin: "PTA13" }, flags: 0x00 },    // port 7  = PTA13 (PWM)
        8: { port: { type: 1, pin: "PTD5" }, flags: 0x00 },     // port 8  = PTD5 (PWM)
        9: { port: { type: 1, pin: "PTD0" }, flags: 0x00 },     // port 9  = PTD0 (PWM)
        10: { port: { type: 2, pin: "PTD3" }, flags: 0x00 },    // port 10 = PTD3 (PWM)
        11: { port: { type: 2, pin: "PTD2" }, flags: 0x00 },    // port 11 = PTD2 (Digital)
        12: { port: { type: 2, pin: "PTC8" }, flags: 0x00 },    // port 12 = PTC8 (Digital)
        13: { port: { type: 2, pin: "PTC9" }, flags: 0x00 },    // port 13 = PTC9 (Digital)
        14: { port: { type: 2, pin: "PTC7" }, flags: 0x00 },    // port 14 = PTC7 (Digital)
        15: { port: { type: 2, pin: "PTC0" }, flags: 0x00 },    // port 15 = PTC0 (Digital)
        16: { port: { type: 2, pin: "PTC3" }, flags: 0x00 },    // port 16 = PTC3 (Digital)
        17: { port: { type: 2, pin: "PTC4" }, flags: 0x00 },    // port 17 = PTC4 (Digital)
        18: { port: { type: 2, pin: "PTC5" }, flags: 0x00 },    // port 18 = PTC5 (Digital)
        19: { port: { type: 2, pin: "PTC6" }, flags: 0x00 },    // port 19 = PTC6 (Digital)
        20: { port: { type: 2, pin: "PTC10" }, flags: 0x00 },   // port 20 = PTC10 (Digital)
        21: { port: { type: 2, pin: "PTC11" }, flags: 0x00 },   // port 21 = PTC11 (Digital)
        22: { port: { type: 2, pin: "PTE0" }, flags: 0x00 }     // port 22 = PTE0 (Digital)
    }
};
var standaloneFactoryXConfig = { };

// Factory configuration for the expansion boards
var expansionBoardFactoryConfig = {
    expansionBoards: {
        version: "0",
        ext0: "1",
        ext1: "1",
        ext2: "0",
        ext3: "0"
    },
    calButtonPins: {
        enabled: 0x03,
        button: "PTE29",
        led: "PTE23"
    },
    TVon: {
        statusPin: "PTD2",
        latchPin: "PTE0",
        relayPin: "PTD3",
        delay: 700
    },
    TLC5940: {
        SIN: "PTC6",
        SCLK: "PTC5",
        XLAT: "PTC10",
        BLANK: "PTC7",
        GSCLK: "PTA1"
    },
    HC595: {
        SIN: "PTA5",
        SCLK: "PTA4",
        LATCH: "PTA12",
        ENA: "PTD4"
    },
    TLC59116: {
        chipMask: 0,
        SDA: "NC",
        SCL: "NC",
        RESET: "NC"
    },
    ZBLaunchBall: {
        port: 0,
        keytype: 2,             // keyboard key
        keycode: 0x28,          // Enter key
        pushDistance: 63        // .063" ~ 1/16"
    },
    IRRemote: {
        sensorPin: "PTA13",
        ledPin: "PTC9"
    },
    buttons: {
        // for expansion board mode, use keyboard mappings for the standard VP and VPinMAME keys
        1:  { pin: "PTC2",  keytype: 2, keycode: 0x1E, flags: 0 },  // "1" = start
        2:  { pin: "PTB3",  keytype: 2, keycode: 0x1F, flags: 0 },  // "2" = extra ball
        3:  { pin: "PTB2",  keytype: 2, keycode: 0x22, flags: 0 },  // "5" = coin 1
        4:  { pin: "PTB1",  keytype: 2, keycode: 0x21, flags: 0 },  // "4" = coin 2
        5:  { pin: "PTE30", keytype: 2, keycode: 0x23, flags: 0 },  // "6" = coin 4
        6:  { pin: "PTC11", keytype: 2, keycode: 0x28, flags: 0 },  // Enter = launch ball
        7:  { pin: "PTE5",  keytype: 2, keycode: 0x29, flags: 0 },  // Escape = exit
        8:  { pin: "PTE4",  keytype: 2, keycode: 0x14, flags: 0 },  // "Q" = quit 
        9:  { pin: "PTE3",  keytype: 2, keycode: 0xE1, flags: 0 },  // left shift = left flipper 
        10: { pin: "PTE2",  keytype: 2, keycode: 0xE5, flags: 0 },  // right shift = right flipper
        11: { pin: "PTB11", keytype: 2, keycode: 0xE0, flags: 0 },  // left control = left magna
        12: { pin: "PTB10", keytype: 2, keycode: 0xE4, flags: 0 },  // right control = right magna 
        13: { pin: "PTB9",  keytype: 2, keycode: 0x17, flags: 0 },  // "T" = tilt bob  
        14: { pin: "PTB8",  keytype: 2, keycode: 0x4A, flags: 0 },  // Home = slam tilt switch
        15: { pin: "PTC12", keytype: 2, keycode: 0x2C, flags: 0 },  // Space = keyboard nudge
        16: { pin: "PTC13", keytype: 2, keycode: 0x4D, flags: 0 },  // "End" = coin door
        17: { pin: "PTC16", keytype: 2, keycode: 0x24, flags: 0 },  // "7" = service escape
        18: { pin: "PTC17", keytype: 2, keycode: 0x25, flags: 0 },  // "8" = service down/-
        19: { pin: "PTA16", keytype: 2, keycode: 0x26, flags: 0 },  // "9" = service up/+
        20: { pin: "PTA17", keytype: 2, keycode: 0x27, flags: 0 },  // "0" = service enter
        21: { pin: "PTE31", keytype: 2, keycode: 0x2E, flags: 0 },  // "=" = VP volume down
        22: { pin: "PTD6",  keytype: 2, keycode: 0x2D, flags: 0 },  // "-" = VP volume up
        23: { pin: "PTD7",  keytype: 3, keycode: 0xE9, flags: 0 },  // media volume up
        24: { pin: "PTE1",  keytype: 3, keycode: 0xEA, flags: 0 }   // media volume down
    },
    outputs: {
        // Map the first 16 ports to the flashers & strobe on TLC5940 #1.
        // These are among the most common devices, so we want to map them
        // into the first 32 logical port numbers for LedWiz compatibility.
        1:  { port: { type: 3, pin: 0  }, flags: 0x04 },
        2:  { port: { type: 3, pin: 1  }, flags: 0x04 },
        3:  { port: { type: 3, pin: 2  }, flags: 0x04 },
        4:  { port: { type: 3, pin: 3  }, flags: 0x04 },
        5:  { port: { type: 3, pin: 4  }, flags: 0x04 },
        6:  { port: { type: 3, pin: 5  }, flags: 0x04 },
        7:  { port: { type: 3, pin: 6  }, flags: 0x04 },
        8:  { port: { type: 3, pin: 7  }, flags: 0x04 },
        9:  { port: { type: 3, pin: 8  }, flags: 0x04 },
        10: { port: { type: 3, pin: 9  }, flags: 0x04 },
        11: { port: { type: 3, pin: 10 }, flags: 0x04 },
        12: { port: { type: 3, pin: 11 }, flags: 0x04 },
        13: { port: { type: 3, pin: 12 }, flags: 0x04 },
        14: { port: { type: 3, pin: 13 }, flags: 0x04 },
        15: { port: { type: 3, pin: 14 }, flags: 0x04 },
        16: { port: { type: 3, pin: 15 }, flags: 0x04 },

        // Map port 17 to the knocker output, since it shares the jumper with the
        // strobe on output 16 - that makes these two physically adjacent outputs
        // logically adjacent in the port mapping.  Knockers are also quite common,
        // so this belongs in the first 32 ports anyway.  Mark it as noisy.
        17: { port: { type: 2, pin: "PTC8"}, flags: 0x02 },

        // Map the next 32 ports to the outputs from the first power board.  These
        // are TLC5940 chips #3 and #4 in the daisy chain (the main board has #1
        // and #2).  This mapping will provide an additional 15 general-purpose 
        // outputs that are accessible through the legacy LedWiz protocol, plus 17
        // more that aren't (because the ports are numbered 33 and up).  Once we
        // get above port 32 (the highest LedWiz-compatible port number), there are
        // no external constraints on port numbering, so we might as well just keep
        // going with contiguous port numbering for the remaining power board outputs.
        // This gives us consecutive DOF numbering for all 32 outputs on the board.
        //
        // These are general-purpose outputs, so don't use gamma.
        18: { port: { type: 3, pin: 32 }, flags: 0x00 },
        19: { port: { type: 3, pin: 33 }, flags: 0x00 },
        20: { port: { type: 3, pin: 34 }, flags: 0x00 },
        21: { port: { type: 3, pin: 35 }, flags: 0x00 },
        22: { port: { type: 3, pin: 36 }, flags: 0x00 },
        23: { port: { type: 3, pin: 37 }, flags: 0x00 },
        24: { port: { type: 3, pin: 38 }, flags: 0x00 },
        25: { port: { type: 3, pin: 39 }, flags: 0x00 },
        26: { port: { type: 3, pin: 40 }, flags: 0x00 },
        27: { port: { type: 3, pin: 41 }, flags: 0x00 },
        28: { port: { type: 3, pin: 42 }, flags: 0x00 },
        29: { port: { type: 3, pin: 43 }, flags: 0x00 },
        30: { port: { type: 3, pin: 44 }, flags: 0x00 },
        31: { port: { type: 3, pin: 45 }, flags: 0x00 },
        32: { port: { type: 3, pin: 46 }, flags: 0x00 },
        33: { port: { type: 3, pin: 47 }, flags: 0x00 },
        34: { port: { type: 3, pin: 48 }, flags: 0x00 },
        35: { port: { type: 3, pin: 49 }, flags: 0x00 },
        36: { port: { type: 3, pin: 50 }, flags: 0x00 },
        37: { port: { type: 3, pin: 51 }, flags: 0x00 },
        38: { port: { type: 3, pin: 52 }, flags: 0x00 },
        39: { port: { type: 3, pin: 53 }, flags: 0x00 },
        40: { port: { type: 3, pin: 54 }, flags: 0x00 },
        41: { port: { type: 3, pin: 55 }, flags: 0x00 },
        42: { port: { type: 3, pin: 56 }, flags: 0x00 },
        43: { port: { type: 3, pin: 57 }, flags: 0x00 },
        44: { port: { type: 3, pin: 58 }, flags: 0x00 },
        45: { port: { type: 3, pin: 59 }, flags: 0x00 },
        46: { port: { type: 3, pin: 60 }, flags: 0x00 },
        47: { port: { type: 3, pin: 61 }, flags: 0x00 },
        48: { port: { type: 3, pin: 62 }, flags: 0x00 },
        49: { port: { type: 3, pin: 63 }, flags: 0x00 },

        // Map the main board flipper button light/small LED outputs next.
        // These are the 16 outputs from the TLC5940 #2.  Flipper button
        // lights aren't among the most common toys, so we deliberately
        // map these in the high number range (where old LedWiz software
        // won't be able to access them) to make room for more common toys
        // in the LedWiz range.
        //
        // These are meant for LEDs, so use gamma by default.
        50: { port: { type: 3, pin: 16 }, flags: 0x04 },
        51: { port: { type: 3, pin: 17 }, flags: 0x04 },
        52: { port: { type: 3, pin: 18 }, flags: 0x04 },
        53: { port: { type: 3, pin: 19 }, flags: 0x04 },
        54: { port: { type: 3, pin: 20 }, flags: 0x04 },
        55: { port: { type: 3, pin: 21 }, flags: 0x04 },
        56: { port: { type: 3, pin: 22 }, flags: 0x04 },
        57: { port: { type: 3, pin: 23 }, flags: 0x04 },
        58: { port: { type: 3, pin: 24 }, flags: 0x04 },
        59: { port: { type: 3, pin: 25 }, flags: 0x04 },
        60: { port: { type: 3, pin: 26 }, flags: 0x04 },
        61: { port: { type: 3, pin: 27 }, flags: 0x04 },
        62: { port: { type: 3, pin: 28 }, flags: 0x04 },
        63: { port: { type: 3, pin: 29 }, flags: 0x04 },
        64: { port: { type: 3, pin: 30 }, flags: 0x04 },
        65: { port: { type: 3, pin: 31 }, flags: 0x04 }
    }
};
var expansionBoardFactoryXConfig = { };


// Factory configuration for the Pinscape AIO
var pinscapeAIOFactoryConfig = {
    expansionBoards: {
        version: "0",
        ext0: "1",
        ext1: "0",
        ext2: "0",
        ext3: "0"
    },
    calButtonPins: {
        enabled: 0x03,
        button: "PTE29",
        led: "PTE23"
    },
    TVon: {
        statusPin: "PTD2",
        latchPin: "PTE0",
        relayPin: "PTD3",
        delay: 700
    },
    TLC5940: {
        SIN: "PTC6",
        SCLK: "PTC5",
        XLAT: "PTC10",
        BLANK: "PTC7",
        GSCLK: "PTA1"
    },
    HC595: {
        SIN: "PTA5",
        SCLK: "PTA4",
        LATCH: "PTA12",
        ENA: "PTD4"
    },
    TLC59116: {
        chipMask: 0,
        SDA: "NC",
        SCL: "NC",
        RESET: "NC"
    },
    ZBLaunchBall: {
        port: 0,
        keytype: 2,             // keyboard key
        keycode: 0x28,          // Enter key
        pushDistance: 63        // .063" ~ 1/16"
    },
    IRRemote: {
        sensorPin: "PTA13",
        ledPin: "PTC9"
    },
    buttons: {
        // for expansion board mode, use keyboard mappings for the standard VP and VPinMAME keys
        1:  { pin: "PTC2",  keytype: 2, keycode: 0x1E, flags: 0 },  // "1" = start
        2:  { pin: "PTB3",  keytype: 2, keycode: 0x1F, flags: 0 },  // "2" = extra ball
        3:  { pin: "PTB2",  keytype: 2, keycode: 0x22, flags: 0 },  // "5" = coin 1
        4:  { pin: "PTB1",  keytype: 2, keycode: 0x21, flags: 0 },  // "4" = coin 2
        5:  { pin: "PTE30", keytype: 2, keycode: 0x23, flags: 0 },  // "6" = coin 4
        6:  { pin: "PTC11", keytype: 2, keycode: 0x28, flags: 0 },  // Enter = launch ball
        7:  { pin: "PTE5",  keytype: 2, keycode: 0x29, flags: 0 },  // Escape = exit
        8:  { pin: "PTE4",  keytype: 2, keycode: 0x14, flags: 0 },  // "Q" = quit 
        9:  { pin: "PTE3",  keytype: 2, keycode: 0xE1, flags: 0 },  // left shift = left flipper 
        10: { pin: "PTE2",  keytype: 2, keycode: 0xE5, flags: 0 },  // right shift = right flipper
        11: { pin: "PTB11", keytype: 2, keycode: 0xE0, flags: 0 },  // left control = left magna
        12: { pin: "PTB10", keytype: 2, keycode: 0xE4, flags: 0 },  // right control = right magna 
        13: { pin: "PTB9",  keytype: 2, keycode: 0x17, flags: 0 },  // "T" = tilt bob  
        14: { pin: "PTB8",  keytype: 2, keycode: 0x4A, flags: 0 },  // Home = slam tilt switch
        15: { pin: "PTC12", keytype: 2, keycode: 0x2C, flags: 0 },  // Space = keyboard nudge
        16: { pin: "PTC13", keytype: 2, keycode: 0x4D, flags: 0 },  // "End" = coin door
        17: { pin: "PTC16", keytype: 2, keycode: 0x24, flags: 0 },  // "7" = service escape
        18: { pin: "PTC17", keytype: 2, keycode: 0x25, flags: 0 },  // "8" = service down/-
        19: { pin: "PTA16", keytype: 2, keycode: 0x26, flags: 0 },  // "9" = service up/+
        20: { pin: "PTA17", keytype: 2, keycode: 0x27, flags: 0 },  // "0" = service enter
        21: { pin: "PTE31", keytype: 2, keycode: 0x2E, flags: 0 },  // "=" = VP volume down
        22: { pin: "PTD6",  keytype: 2, keycode: 0x2D, flags: 0 },  // "-" = VP volume up
        23: { pin: "PTD7",  keytype: 3, keycode: 0xE9, flags: 0 },  // media volume up
        24: { pin: "PTE1",  keytype: 3, keycode: 0xEA, flags: 0 }   // media volume down
    },
    outputs: {
        // Map the first 16 ports to the flashers & strobe on TLC5940 #1.
        // These are among the most common devices, so we want to map them
        // into the first 32 logical port numbers for LedWiz compatibility.
        1:  { port: { type: 3, pin: 0  }, flags: 0x04 },
        2:  { port: { type: 3, pin: 1  }, flags: 0x04 },
        3:  { port: { type: 3, pin: 2  }, flags: 0x04 },
        4:  { port: { type: 3, pin: 3  }, flags: 0x04 },
        5:  { port: { type: 3, pin: 4  }, flags: 0x04 },
        6:  { port: { type: 3, pin: 5  }, flags: 0x04 },
        7:  { port: { type: 3, pin: 6  }, flags: 0x04 },
        8:  { port: { type: 3, pin: 7  }, flags: 0x04 },
        9:  { port: { type: 3, pin: 8  }, flags: 0x04 },
        10: { port: { type: 3, pin: 9  }, flags: 0x04 },
        11: { port: { type: 3, pin: 10 }, flags: 0x04 },
        12: { port: { type: 3, pin: 11 }, flags: 0x04 },
        13: { port: { type: 3, pin: 12 }, flags: 0x04 },
        14: { port: { type: 3, pin: 13 }, flags: 0x04 },
        15: { port: { type: 3, pin: 14 }, flags: 0x04 },
        16: { port: { type: 3, pin: 15 }, flags: 0x04 },

        // Map port 17 to the knocker output, since it shares the jumper with the
        // strobe on output 16 - that makes these two physically adjacent outputs
        // logically adjacent in the port mapping.  Knockers are also quite common,
        // so this belongs in the first 32 ports anyway.  Mark it as noisy.
        17: { port: { type: 2, pin: "PTC8"}, flags: 0x02 },

        // Map the next 32 ports to the outputs from the first power board.  These
        // are TLC5940 chips #3 and #4 in the daisy chain (the main board has #1
        // and #2).  This mapping will provide an additional 15 general-purpose 
        // outputs that are accessible through the legacy LedWiz protocol, plus 17
        // more that aren't (because the ports are numbered 33 and up).  Once we
        // get above port 32 (the highest LedWiz-compatible port number), there are
        // no external constraints on port numbering, so we might as well just keep
        // going with contiguous port numbering for the remaining power board outputs.
        // This gives us consecutive DOF numbering for all 32 outputs on the board.
        //
        // These are general-purpose outputs, so don't use gamma.
        18: { port: { type: 3, pin: 32 }, flags: 0x00 },
        19: { port: { type: 3, pin: 33 }, flags: 0x00 },
        20: { port: { type: 3, pin: 34 }, flags: 0x00 },
        21: { port: { type: 3, pin: 35 }, flags: 0x00 },
        22: { port: { type: 3, pin: 36 }, flags: 0x00 },
        23: { port: { type: 3, pin: 37 }, flags: 0x00 },
        24: { port: { type: 3, pin: 38 }, flags: 0x00 },
        25: { port: { type: 3, pin: 39 }, flags: 0x00 },
        26: { port: { type: 3, pin: 40 }, flags: 0x00 },
        27: { port: { type: 3, pin: 41 }, flags: 0x00 },
        28: { port: { type: 3, pin: 42 }, flags: 0x00 },
        29: { port: { type: 3, pin: 43 }, flags: 0x00 },
        30: { port: { type: 3, pin: 44 }, flags: 0x00 },
        31: { port: { type: 3, pin: 45 }, flags: 0x00 },
        32: { port: { type: 3, pin: 46 }, flags: 0x00 },
        33: { port: { type: 3, pin: 47 }, flags: 0x00 },
        34: { port: { type: 3, pin: 48 }, flags: 0x00 },
        35: { port: { type: 3, pin: 49 }, flags: 0x00 },
        36: { port: { type: 3, pin: 50 }, flags: 0x00 },
        37: { port: { type: 3, pin: 51 }, flags: 0x00 },
        38: { port: { type: 3, pin: 52 }, flags: 0x00 },
        39: { port: { type: 3, pin: 53 }, flags: 0x00 },
        40: { port: { type: 3, pin: 54 }, flags: 0x00 },
        41: { port: { type: 3, pin: 55 }, flags: 0x00 },
        42: { port: { type: 3, pin: 56 }, flags: 0x00 },
        43: { port: { type: 3, pin: 57 }, flags: 0x00 },
        44: { port: { type: 3, pin: 58 }, flags: 0x00 },
        45: { port: { type: 3, pin: 59 }, flags: 0x00 },
        46: { port: { type: 3, pin: 60 }, flags: 0x00 },
        47: { port: { type: 3, pin: 61 }, flags: 0x00 },
        48: { port: { type: 3, pin: 62 }, flags: 0x00 },
        49: { port: { type: 3, pin: 63 }, flags: 0x00 },

        // Map the main board flipper button light/small LED outputs next.
        // These are the 16 outputs from the TLC5940 #2.  Flipper button
        // lights aren't among the most common toys, so we deliberately
        // map these in the high number range (where old LedWiz software
        // won't be able to access them) to make room for more common toys
        // in the LedWiz range.
        //
        // These are meant for LEDs, so use gamma by default.
        50: { port: { type: 3, pin: 16 }, flags: 0x04 },
        51: { port: { type: 3, pin: 17 }, flags: 0x04 },
        52: { port: { type: 3, pin: 18 }, flags: 0x04 },
        53: { port: { type: 3, pin: 19 }, flags: 0x04 },
        54: { port: { type: 3, pin: 20 }, flags: 0x04 },
        55: { port: { type: 3, pin: 21 }, flags: 0x04 },
        56: { port: { type: 3, pin: 22 }, flags: 0x04 },
        57: { port: { type: 3, pin: 23 }, flags: 0x04 },
        58: { port: { type: 3, pin: 24 }, flags: 0x04 },
        59: { port: { type: 3, pin: 25 }, flags: 0x04 },
        60: { port: { type: 3, pin: 26 }, flags: 0x04 },
        61: { port: { type: 3, pin: 27 }, flags: 0x04 },
        62: { port: { type: 3, pin: 28 }, flags: 0x04 },
        63: { port: { type: 3, pin: 29 }, flags: 0x04 },
        64: { port: { type: 3, pin: 30 }, flags: 0x04 },
        65: { port: { type: 3, pin: 31 }, flags: 0x04 },

        // Map the main board timed outputs
        //
        // These are meant for noisy outputs so mark it as noisy.
        66: { port: { type: 4, pin: 0 }, flags: 0x02 },
        67: { port: { type: 4, pin: 1 }, flags: 0x02 },
        68: { port: { type: 4, pin: 2 }, flags: 0x02 },
        69: { port: { type: 4, pin: 3 }, flags: 0x02 },
        70: { port: { type: 4, pin: 4 }, flags: 0x02 },
        71: { port: { type: 4, pin: 5 }, flags: 0x02 },
        72: { port: { type: 4, pin: 6 }, flags: 0x02 },
        73: { port: { type: 4, pin: 7 }, flags: 0x02 }
    }
};
var pinscapeAIOFactoryXConfig = { };

// Factory configuration for the Pinscape Lite
var pinscapeLiteFactoryConfig = {
    expansionBoards: {
        version: "0",
        ext0: "1",
        ext1: "0",
        ext2: "0",
        ext3: "0"
    },
    calButtonPins: {
        enabled: 0x0,
        button: "NC",
        led: "NC"
    },
    TVon: {
        statusPin: "NC",
        latchPin: "NC",
        relayPin: "NC",
        delay: 0
    },
    TLC5940: {
        SIN: "PTC6",
        SCLK: "PTC5",
        XLAT: "PTC10",
        BLANK: "PTC7",
        GSCLK: "PTA1"
    },
    HC595: {
        SIN: "PTA5",
        SCLK: "PTA4",
        LATCH: "PTA12",
        ENA: "PTD4"
    },
    TLC59116: {
        chipMask: 0,
        SDA: "NC",
        SCL: "NC",
        RESET: "NC"
    },
    ZBLaunchBall: {
        port: 0,
        keytype: 2,             // keyboard key
        keycode: 0x28,          // Enter key
        pushDistance: 63        // .063" ~ 1/16"
    },
    buttons: {
        // for expansion board mode, use keyboard mappings for the standard VP and VPinMAME keys
        1: { pin: "PTC2", keytype: 2, keycode: 0x1E, flags: 0 },  // "1" = start
        2: { pin: "PTB3", keytype: 2, keycode: 0x1F, flags: 0 },  // "2" = extra ball
        3: { pin: "PTB2", keytype: 2, keycode: 0x22, flags: 0 },  // "5" = coin 1
        4: { pin: "PTB1", keytype: 2, keycode: 0x21, flags: 0 },  // "4" = coin 2
        5: { pin: "PTE30", keytype: 2, keycode: 0x23, flags: 0 },  // "6" = coin 4
        6: { pin: "PTC11", keytype: 2, keycode: 0x28, flags: 0 },  // Enter = launch ball
        7: { pin: "PTE5", keytype: 2, keycode: 0x29, flags: 0 },  // Escape = exit
        8: { pin: "PTE4", keytype: 2, keycode: 0x14, flags: 0 },  // "Q" = quit 
        9: { pin: "PTE3", keytype: 2, keycode: 0xE1, flags: 0 },  // left shift = left flipper 
        10: { pin: "PTE2", keytype: 2, keycode: 0xE5, flags: 0 },  // right shift = right flipper
        11: { pin: "PTB11", keytype: 2, keycode: 0xE0, flags: 0 },  // left control = left magna
        12: { pin: "PTB10", keytype: 2, keycode: 0xE4, flags: 0 },  // right control = right magna 
        13: { pin: "PTB9", keytype: 2, keycode: 0x17, flags: 0 },  // "T" = tilt bob  
        14: { pin: "PTB8", keytype: 2, keycode: 0x4A, flags: 0 },  // Home = slam tilt switch
        15: { pin: "PTC12", keytype: 2, keycode: 0x2C, flags: 0 },  // Space = keyboard nudge
        16: { pin: "PTC13", keytype: 2, keycode: 0x4D, flags: 0 },  // "End" = coin door
        17: { pin: "PTC16", keytype: 2, keycode: 0x24, flags: 0 },  // "7" = service escape
        18: { pin: "PTC17", keytype: 2, keycode: 0x25, flags: 0 },  // "8" = service down/-
        19: { pin: "PTA16", keytype: 2, keycode: 0x26, flags: 0 },  // "9" = service up/+
        20: { pin: "PTA17", keytype: 2, keycode: 0x27, flags: 0 },  // "0" = service enter
        21: { pin: "PTE31", keytype: 2, keycode: 0x2E, flags: 0 },  // "=" = VP volume down
        22: { pin: "PTD6", keytype: 2, keycode: 0x2D, flags: 0 },  // "-" = VP volume up
        23: { pin: "PTD7", keytype: 3, keycode: 0xE9, flags: 0 },  // media volume up
        24: { pin: "PTE1", keytype: 3, keycode: 0xEA, flags: 0 }   // media volume down
    },
    outputs: {
        // Map the 12 GPIOs that connect to MOSFET power outputs
        // Only the first 2 are PWM capable
        1: { port: { type: 1, pin: "PTA2" }, flags: 0x04 },     // port 1  = PTA2 (PWM)
        2: { port: { type: 1, pin: "PTA13" }, flags: 0x04 },    // port 2  = PTA13 (PWM)
        3: { port: { type: 2, pin: "PTD2" }, flags: 0x00 },     // port 3 = PTD2 (Digital)
        4: { port: { type: 2, pin: "PTD3" }, flags: 0x00 },     // port 4 = PTD3 (Digital)
        5: { port: { type: 2, pin: "PTC8" }, flags: 0x00 },     // port 5 = PTC8 (Digital)
        6: { port: { type: 2, pin: "PTC9" }, flags: 0x00 },     // port 6 = PTC9 (Digital)
        7: { port: { type: 2, pin: "PTC0" }, flags: 0x00 },     // port 7 = PTC0 (Digital)
        8: { port: { type: 2, pin: "PTC3" }, flags: 0x00 },     // port 8 = PTC3 (Digital)
        9: { port: { type: 2, pin: "PTC4" }, flags: 0x00 },     // port 9 = PTC4 (Digital)
        10: { port: { type: 2, pin: "PTE23" }, flags: 0x00 },   // port 10 = PTE23 (Digital)
        11: { port: { type: 2, pin: "PTE29" }, flags: 0x00 },   // port 11 = PTE29 (Digital)
        12: { port: { type: 2, pin: "PTE0" }, flags: 0x00 },    // port 12 = PTE0 (Digital)

        // Map the first 16 ports to the board flipper button light/small LED outputs.
        // These are the 16 outputs from the TLC5940 #1
        // These are meant for LEDs, so use gamma by default.
        13: { port: { type: 3, pin: 0 }, flags: 0x04 },
        14: { port: { type: 3, pin: 1 }, flags: 0x04 },
        15: { port: { type: 3, pin: 2 }, flags: 0x04 },
        16: { port: { type: 3, pin: 3 }, flags: 0x04 },
        17: { port: { type: 3, pin: 4 }, flags: 0x04 },
        18: { port: { type: 3, pin: 5 }, flags: 0x04 },
        19: { port: { type: 3, pin: 6 }, flags: 0x04 },
        20: { port: { type: 3, pin: 7 }, flags: 0x04 },
        21: { port: { type: 3, pin: 8 }, flags: 0x04 },
        22: { port: { type: 3, pin: 9 }, flags: 0x04 },
        23: { port: { type: 3, pin: 10 }, flags: 0x04 },
        24: { port: { type: 3, pin: 11 }, flags: 0x04 },
        25: { port: { type: 3, pin: 12 }, flags: 0x04 },
        26: { port: { type: 3, pin: 13 }, flags: 0x04 },
        27: { port: { type: 3, pin: 14 }, flags: 0x04 },
        28: { port: { type: 3, pin: 15 }, flags: 0x04 }
    }
};
var pinscapeLiteFactoryXConfig = {};

// Factory configuration for Arnoz RigMaster
var rigMasterFactoryConfig = {
    expansionBoards: {
        version: "0",
        ext0: "1",
        ext1: "0",
        ext2: "0",
        ext3: "0"
    },
    calButtonPins: {
        enabled: 0x0,
        button: "PTC12",
        led: "PTC13"
    },
    nightMode: {
        button: 19,
        flags: 0,
        output: 0
    },
    plungerType: {
        type: 0,
        param1: 0,
    },
    TVon: {
        statusPin: "NC",
        latchPin: "NC",
        relayPin: "NC",
        delay: 0
    },
    TLC5940: {
        SIN: "PTC6",
        SCLK: "PTC5",
        XLAT: "PTC10",
        BLANK: "PTC11",
        GSCLK: "PTA13"
    },
    HC595: {
        SIN: "NC",
        SCLK: "NC",
        LATCH: "NC",
        ENA: "NC"
    },
    TLC59116: {
        chipMask: 0,
        SDA: "NC",
        SCL: "NC",
        RESET: "NC"
    },
    ZBLaunchBall: {
        port: 0,
        keytype: 2,             // keyboard key
        keycode: 0x28,          // Enter key
        pushDistance: 63        // .063" ~ 1/16"
    },
    buttons: {
        // for expansion board mode, use keyboard mappings for the standard VP and VPinMAME keys
        1: { pin: "PTE30", keytype: 2, keycode: 0x1E, flags: 0 },  // "1" = start
        2: { pin: "PTE29", keytype: 2, keycode: 0x1F, flags: 0 },  // "2" = extra ball
        3: { pin: "PTE23", keytype: 2, keycode: 0x22, flags: 0 },  // "5" = coin 1
        4: { pin: "PTE22", keytype: 2, keycode: 0x21, flags: 0 },  // "4" = coin 2
        5: { pin: "PTE21", keytype: 2, keycode: 0x28, flags: 0 },  // Enter = launch ball
        6: { pin: "PTE20", keytype: 2, keycode: 0x29, flags: 0 },  // // Escape = exit
        7: { pin: "PTE5", keytype: 2, keycode: 0x14, flags: 0 },  // "Q" = quit 
        8: { pin: "PTE4", keytype: 2, keycode: 0xE1, flags: 0 },  // left shift = left flipper 
        9: { pin: "PTE3", keytype: 2, keycode: 0xE5, flags: 0 },  // right shift = right flipper 
        10: { pin: "PTE2", keytype: 2, keycode: 0xE0, flags: 0 },  // left control = left magna
        11: { pin: "PTE1", keytype: 2, keycode: 0xE4, flags: 0 },  // right control = right magna 
        12: { pin: "PTE0", keytype: 2, keycode: 0x76, flags: 0 },  // Fire 
        13: { pin: "PTD7", keytype: 2, keycode: 0x17, flags: 0 },  // "T" = tilt bob  
        14: { pin: "PTD6", keytype: 2, keycode: 0x4D, flags: 0 },  // // "End" = coin door
        15: { pin: "PTD5", keytype: 2, keycode: 0x24, flags: 0 },  // "7" = service escape
        16: { pin: "PTD4", keytype: 2, keycode: 0x25, flags: 0 },  // "8" = service down/-
        17: { pin: "PTD3", keytype: 2, keycode: 0x26, flags: 0 },  // "9" = service up/+
        18: { pin: "PTD2", keytype: 2, keycode: 0x27, flags: 0 },  // "0" = service enter
        19: { pin: "PTC16", keytype: 0, keycode: 0, flags: 0 },  // NightMode
        20: { pin: "PTC7", keytype: 3, keycode: 0xEA, flags: 0 },  // Volume-
        21: { pin: "PTC17", keytype: 3, keycode: 0xE9, flags: 0 },  // Volume+
    },
    outputs: {
        // Map the 16 GPIOs that connect to MOSFET power outputs
        // Only the last 8 are PWM capable
        1: { port: { type: 2, pin: "PTB1" }, flags: 0x04 },     // port 1  = PTB1 (Digital)
        2: { port: { type: 2, pin: "PTB2" }, flags: 0x04 },    // port 2  = PTB2 (Digital)
        3: { port: { type: 2, pin: "PTB3" }, flags: 0x00 },     // port 3 = PTB3 (Digital)
        4: { port: { type: 2, pin: "PTB8" }, flags: 0x00 },     // port 4 = PTD3 (Digital)
        5: { port: { type: 2, pin: "PTB9" }, flags: 0x00 },     // port 5 = PTB8 (Digital)
        6: { port: { type: 2, pin: "PTB10" }, flags: 0x00 },     // port 6 = PTB10 (Digital)
        7: { port: { type: 2, pin: "PTB11" }, flags: 0x00 },     // port 7 = PTB11 (Digital)
        8: { port: { type: 2, pin: "PTA17" }, flags: 0x00 },     // port 8 = PTA17 (Digital)
        9: { port: { type: 1, pin: "PTA1" }, flags: 0x00 },     // port 9 = PTA1 (PWM)
        10: { port: { type: 1, pin: "PTA2" }, flags: 0x00 },   // port 10 = PTA2 (PWM)
        11: { port: { type: 1, pin: "PTA4" }, flags: 0x00 },   // port 11 = PTA4 (PWM)
        12: { port: { type: 1, pin: "PTA5" }, flags: 0x00 },    // port 12 = PTA5 (PWM)
        13: { port: { type: 1, pin: "PTD0" }, flags: 0x00 },    // port 13 = " (PWM)
        14: { port: { type: 1, pin: "PTC4" }, flags: 0x00 },    // port 14 = PTC4 (PWM)
        15: { port: { type: 1, pin: "PTC8" }, flags: 0x00 },    // port 15 = PTC8 (PWM)
        16: { port: { type: 1, pin: "PTC9" }, flags: 0x00 },    // port 16 = PTC9 (PWM)

    }
};
var rigMasterFactoryXConfig = {};

// Factory configuration for Arnoz KLShield
var klShieldFactoryConfig = {
    expansionBoards: {
        version: "0",
        ext0: "1",
        ext1: "0",
        ext2: "0",
        ext3: "0"
    },
    calButtonPins: {
        enabled: 0x0,
        button: "PTC12",
        led: "PTC13"
    },
    nightMode: {
        button: 19,
        flags: 0,
        output: 0
    },
    plungerType: {
        type: 0,
        param1: 0,
    },
    TVon: {
        statusPin: "NC",
        latchPin: "NC",
        relayPin: "NC",
        delay: 0
    },
    TLC5940: {
        SIN: "PTC6",
        SCLK: "PTC5",
        XLAT: "PTC10",
        BLANK: "PTC11",
        GSCLK: "PTA13"
    },
    HC595: {
        SIN: "NC",
        SCLK: "NC",
        LATCH: "NC",
        ENA: "NC"
    },
    TLC59116: {
        chipMask: 0,
        SDA: "NC",
        SCL: "NC",
        RESET: "NC"
    },
    ZBLaunchBall: {
        port: 0,
        keytype: 2,             // keyboard key
        keycode: 0x28,          // Enter key
        pushDistance: 63        // .063" ~ 1/16"
    },
    buttons: {
        // for expansion board mode, use keyboard mappings for the standard VP and VPinMAME keys
        1: { pin: "PTE30", keytype: 2, keycode: 0x1E, flags: 0 },  // "1" = start
        2: { pin: "PTE29", keytype: 2, keycode: 0x1F, flags: 0 },  // "2" = extra ball
        3: { pin: "PTE23", keytype: 2, keycode: 0x22, flags: 0 },  // "5" = coin 1
        4: { pin: "PTE22", keytype: 2, keycode: 0x21, flags: 0 },  // "4" = coin 2
        5: { pin: "PTE21", keytype: 2, keycode: 0x28, flags: 0 },  // Enter = launch ball
        6: { pin: "PTE20", keytype: 2, keycode: 0x29, flags: 0 },  // // Escape = exit
        7: { pin: "PTE5", keytype: 2, keycode: 0x14, flags: 0 },  // "Q" = quit 
        8: { pin: "PTE4", keytype: 2, keycode: 0xE1, flags: 0 },  // left shift = left flipper 
        9: { pin: "PTE3", keytype: 2, keycode: 0xE5, flags: 0 },  // right shift = right flipper 
        10: { pin: "PTE2", keytype: 2, keycode: 0xE0, flags: 0 },  // left control = left magna
        11: { pin: "PTE1", keytype: 2, keycode: 0xE4, flags: 0 },  // right control = right magna 
        12: { pin: "PTE0", keytype: 2, keycode: 0x76, flags: 0 },  // Fire 
        13: { pin: "PTD7", keytype: 2, keycode: 0x17, flags: 0 },  // "T" = tilt bob  
        14: { pin: "PTD6", keytype: 2, keycode: 0x4D, flags: 0 },  // // "End" = coin door
        15: { pin: "PTD5", keytype: 2, keycode: 0x24, flags: 0 },  // "7" = service escape
        16: { pin: "PTD4", keytype: 2, keycode: 0x25, flags: 0 },  // "8" = service down/-
        17: { pin: "PTD3", keytype: 2, keycode: 0x26, flags: 0 },  // "9" = service up/+
        18: { pin: "PTD2", keytype: 2, keycode: 0x27, flags: 0 },  // "0" = service enter
        19: { pin: "PTC16", keytype: 0, keycode: 0, flags: 0 },  // NightMode
        20: { pin: "PTC7", keytype: 3, keycode: 0xEA, flags: 0 },  // Volume-
        21: { pin: "PTC17", keytype: 3, keycode: 0xE9, flags: 0 },  // Volume+
    },
    outputs: {

    }
};
var klShieldFactoryXConfig = {};

// fill out the factory defaults for the maximum output and button table sizes
(function() {
    function fill(d) {
        for (var i = 1 ; i <= MaxButtons ; ++i) {
            if (!(i in d.buttons)) {
                d.buttons[i] = { pin: "NC", keytype: 0, keycode: 0, flags: 0 };
            }
        }
        for (var i = 1 ; i <= MaxOutputs ; ++i) {
            if (!(i in d.outputs)) {
                d.outputs[i] = { port: { type: 0, pin: 0 } };
            }
        }
    }
    fill(expansionBoardFactoryConfig);
    fill(pinscapeAIOFactoryConfig);
    fill(pinscapeLiteFactoryConfig);
    fill(rigMasterFactoryConfig);
    fill(klShieldFactoryConfig);
    fill(standaloneFactoryConfig);
})();
    
// Output Port Aliases.  This is a table of port name aliases for
// expansion boards.  With the base configuration, we show output
// port names in raw hardware terms - i.e., the physical pins on
// the KL25Z or peripheral chips such as TLC5940's.  When an expansion
// board is used, the user does all of the wiring through the board
// headers, so we substitute the header pin locations in our displays.
// This table maps from hardware pins to the alias strings.  The keys
// are strings in the form "type.pin", where 'type' is the pin type
// number used in the USB setup messages (1 for KL25Z PWM GPIO, etc)
// and 'pin' is the internal pin name ("PTxx" for GPIO ports, or a
// port number from 0 to N for peripheral chips).
//
// Format of each entry:
//
//   Descriptive name|Output type description|Board name|JPx-y|xxxxOutputSelector
//
var outPortAlias = { };

// GPIO Port Aliases.  This is similar to the outPortAlias table,
// but covers only GPIO ports (no peripheral ports), and covers all
// GPIO ports used for any purpose, including inputs, outputs, and
// control connections to peripherals.
//
// Format:
//
//   Descriptive name|JPx-y or Internal
//
var gpioPortAlias = { };

// build the expansion board out port alias table
var expOutPortAlias, expGpioPortAlias;
(function()
{
    var o = {
        "3.15":   "Strobe|PWM Mid Power|Main Board|JP9-1|mainBoardPWMOutputSelector",

        "3.0":    "Flasher 1R|PWM Mid Power|Main Board|JP11-2|mainBoardPWMOutputSelector",
        "3.1":    "Flasher 1G|PWM Mid Power|Main Board|JP11-4|mainBoardPWMOutputSelector",
        "3.2":    "Flasher 1B|PWM Mid Power|Main Board|JP11-6|mainBoardPWMOutputSelector",
        "3.3":    "Flasher 2R|PWM Mid Power|Main Board|JP11-8|mainBoardPWMOutputSelector",
        "3.4":    "Flasher 2G|PWM Mid Power|Main Board|JP11-10|mainBoardPWMOutputSelector",
        "3.5":    "Flasher 2B|PWM Mid Power|Main Board|JP11-12|mainBoardPWMOutputSelector",
        "3.6":    "Flasher 3R|PWM Mid Power|Main Board|JP11-14|mainBoardPWMOutputSelector",
        "3.7":    "Flasher 3G|PWM Mid Power|Main Board|JP11-16|mainBoardPWMOutputSelector",
        "3.8":    "Flasher 3B|PWM Mid Power|Main Board|JP11-1|mainBoardPWMOutputSelector",
        "3.9":    "Flasher 4R|PWM Mid Power|Main Board|JP11-3|mainBoardPWMOutputSelector",
        "3.10":   "Flasher 4G|PWM Mid Power|Main Board|JP11-5|mainBoardPWMOutputSelector",
        "3.11":   "Flasher 4B|PWM Mid Power|Main Board|JP11-7|mainBoardPWMOutputSelector",
        "3.12":   "Flasher 5R|PWM Mid Power|Main Board|JP11-9|mainBoardPWMOutputSelector",
        "3.13":   "Flasher 5G|PWM Mid Power|Main Board|JP11-11|mainBoardPWMOutputSelector",
        "3.14":   "Flasher 5B|PWM Mid Power|Main Board|JP11-13|mainBoardPWMOutputSelector",

        "3.16":   "LED 1R|PWM Low Power|Main Board|JP8-1|mainBoardPWMOutputSelector",
        "3.17":   "LED 1G|PWM Low Power|Main Board|JP8-3|mainBoardPWMOutputSelector",
        "3.18":   "LED 1B|PWM Low Power|Main Board|JP8-5|mainBoardPWMOutputSelector",
        "3.19":   "LED 2R|PWM Low Power|Main Board|JP8-7|mainBoardPWMOutputSelector",
        "3.20":   "LED 2G|PWM Low Power|Main Board|JP8-9|mainBoardPWMOutputSelector",
        "3.21":   "LED 2B|PWM Low Power|Main Board|JP8-11|mainBoardPWMOutputSelector",
        "3.22":   "LED 3R|PWM Low Power|Main Board|JP8-2|mainBoardPWMOutputSelector",
        "3.23":   "LED 3G|PWM Low Power|Main Board|JP8-4|mainBoardPWMOutputSelector",
        "3.24":   "LED 3B|PWM Low Power|Main Board|JP8-6|mainBoardPWMOutputSelector",
        "3.25":   "LED 4R|PWM Low Power|Main Board|JP8-8|mainBoardPWMOutputSelector",
        "3.26":   "LED 4G|PWM Low Power|Main Board|JP8-10|mainBoardPWMOutputSelector",
        "3.27":   "LED 4B|PWM Low Power|Main Board|JP8-12|mainBoardPWMOutputSelector",
        "3.28":   "LED 5R|PWM Low Power|Main Board|JP8-13|mainBoardPWMOutputSelector",
        "3.29":   "LED 5G|PWM Low Power|Main Board|JP8-15|mainBoardPWMOutputSelector",
        "3.30":   "LED 5B|PWM Low Power|Main Board|JP8-17|mainBoardPWMOutputSelector",
        "3.31":   "LED 6|PWM Low Power|Main Board|JP8-14|mainBoardPWMOutputSelector"
    };

    // add four power boards worth of outputs
    for (var i = 0 ; i < 128 ; ++i) {
        o["3." + (i+32)] =
            "Output " + ((i%32) + 1)
            + "|PWM Hi Power"
            + "|Power Board " + (Math.floor(i/32) + 1)
            + "|JP" + (Math.floor((i%32)/16)+5)+ "-" + ((i%16)+1)
            + "|powerBoardOutputSelector";
    }

    // add four chime boards worth of outputs
    for (i = 0 ; i < 32 ; ++i) {
        o["4." + i] =
            "Output " + ((i%8) + 1)
            + "|Timed Digital"
            + "|Chime Board " + (Math.floor(i/8) + 1)
            + "|JP9-" + ((i%8)+1)
            + "|chimeBoardOutputSelector";
    }

    // Table of all internal GPIO connections on the main expansion board.
    // These generally can't be used for any other purpose.
    var gInternal = {
        "PTC10":  "TLC5940 XLAT",
        "PTC6":   "TLC5940 SIN",
        "PTC5":   "TLC5940 SCLK",
        "PTC7":   "TLC5940 BLANK",
        "PTA1":   "TLC5940 GSCLK",
        "PTC9":   "IR OUT",
        "PTA13":  "IR IN",
        "PTD2":   "PSU2 STATUS",
        "PTD3":   "TV RELAY",
        "PTE0":   "PSU2 LATCH"
    };

    // Table of all external GPIO connections on the main expansion
    // board.  These KL25Z pins are connected more or less directly
    // to external headers on the board, so they can be re-purposed
    // (sometimes with restrictions) for other external uses.  For
    // example, if a plunger sensor isn't being used, all of the
    // plunger input pins can be used as button inputs instead.
    var gExternal = {
        "PTB0":   "Plunger 1|JP2-1",
        "PTE23":  "Cal Button LED|JP3-2",
        "PTE29":  "Cal Button Switch|JP3-1",
        "PTE22":  "Plunger 4|JP2-4",
        "PTD5":   "Plunger 4|JP2-4",
        "PTE21":  "Plunger 3|JP2-3",
        "PTD0":   "Plunger 3|JP2-3",
        "PTE20":  "Plunger 2|JP2-8",
        "PTD4":   "74HC595 ENA|JP5-7",
        "PTA4":   "74HC595 SCLK|JP5-3",
        "PTA5":   "74HC595 SOUT|JP5-1",
        "PTA12":  "74HC595 LATCH|JP5-5",
        "PTC8":   "Knocker|JP9-2|Timed Digital",
        "PTC4":   "Extender 1|JP12-1",
        "PTC3":   "Extender 2|JP12-2",
        "PTC0":   "Extender 3|JP12-3",
        "PTA2":   "Extender 4|JP12-4",
    };

    // Build a combined table of the GPIO port aliases for internal and
    // external main board connections.
    var g = { };
    $.each(gInternal, function(k, v) { g[k] = v + "|Internal"; });
    $.each(gExternal, function(k, v) { g[k] = v.split("|").slice(0, 2).join("|"); });

    // Add GPIO aliases for all of the buttons
    $.each(expansionBoardFactoryConfig.buttons, function(k, v) {
        g[v.pin] = "Button " + k + "|Digital In|Main Board|JP1-" + k + "|mainBoardInputSelector";
    });

    // Add all of the external ports as output aliases.
    $.each(gExternal, function(k, v)
    {
        // break v into fields - Descriptive Name, Jumper, [output type description]
        v = v.split("|");

        // if it's a PWM-capable port, add a PWM output type for it
        var gp = gpioPinsByName[k] || { };
        if (gp.pwm)
            o["1." + k] = [v[0], v[2] || "PWM GPIO", "Main Board", v[1], "mainBoardPWMOutputSelector"].join("|");

        // add a Digital Out type for it
        o["2." + k] = [v[0], v[2] || "Digital GPIO", "Main Board", v[1], "mainBoardDigitalOutputSelector"].join("|");
    });

    // remember these
    expOutPortAlias = o;
    expGpioPortAlias = g;
})();


var aioOutPortAlias, aioGpioPortAlias;
// build the Pinscape AIO out port alias table
(function()
{
    var o = {
        "3.0":    "Flasher 1R|PWM Mid Power|Pinscape AIO|Flasher 1R|aioBoardPWMOutputSelector",
        "3.1":    "Flasher 1G|PWM Mid Power|Pinscape AIO|Flasher 1G|aioBoardPWMOutputSelector",
        "3.2":    "Flasher 1B|PWM Mid Power|Pinscape AIO|Flasher 1B|aioBoardPWMOutputSelector",
        "3.3":    "Flasher 2R|PWM Mid Power|Pinscape AIO|Flasher 2R|aioBoardPWMOutputSelector",
        "3.4":    "Flasher 2G|PWM Mid Power|Pinscape AIO|Flasher 2G|aioBoardPWMOutputSelector",
        "3.5":    "Flasher 2B|PWM Mid Power|Pinscape AIO|Flasher 2B|aioBoardPWMOutputSelector",
        "3.6":    "Flasher 3R|PWM Mid Power|Pinscape AIO|Flasher 3R|aioBoardPWMOutputSelector",
        "3.7":    "Flasher 3G|PWM Mid Power|Pinscape AIO|Flasher 3G|aioBoardPWMOutputSelector",
        "3.8":    "Flasher 3B|PWM Mid Power|Pinscape AIO|Flasher 3B|aioBoardPWMOutputSelector",
        "3.9":    "Flasher 4R|PWM Mid Power|Pinscape AIO|Flasher 4R|aioBoardPWMOutputSelector",
        "3.10":   "Flasher 4G|PWM Mid Power|Pinscape AIO|Flasher 4G|aioBoardPWMOutputSelector",
        "3.11":   "Flasher 4B|PWM Mid Power|Pinscape AIO|Flasher 4B|aioBoardPWMOutputSelector",
        "3.12":   "Flasher 5R|PWM Mid Power|Pinscape AIO|Flasher 5R|aioBoardPWMOutputSelector",
        "3.13":   "Flasher 5G|PWM Mid Power|Pinscape AIO|Flasher 5G|aioBoardPWMOutputSelector",
        "3.14":   "Flasher 5B|PWM Mid Power|Pinscape AIO|Flasher 5B|aioBoardPWMOutputSelector",
        "3.15":   "Strobe|PWM Mid Power|Pinscape AIO|Flasher Strobe|aioBoardPWMOutputSelector",

        "3.16":   "LED 1R|PWM Low Power|Pinscape AIO|Small LED 1R|aioBoardPWMOutputSelector",
        "3.17":   "LED 1G|PWM Low Power|Pinscape AIO|Small LED 1G|aioBoardPWMOutputSelector",
        "3.18":   "LED 1B|PWM Low Power|Pinscape AIO|Small LED 1B|aioBoardPWMOutputSelector",
        "3.19":   "LED 2R|PWM Low Power|Pinscape AIO|Small LED 2R|aioBoardPWMOutputSelector",
        "3.20":   "LED 2G|PWM Low Power|Pinscape AIO|Small LED 2G|aioBoardPWMOutputSelector",
        "3.21":   "LED 2B|PWM Low Power|Pinscape AIO|Small LED 2B|aioBoardPWMOutputSelector",
        "3.22":   "LED 3R|PWM Low Power|Pinscape AIO|Small LED 3R|aioBoardPWMOutputSelector",
        "3.23":   "LED 3G|PWM Low Power|Pinscape AIO|Small LED 3G|aioBoardPWMOutputSelector",
        "3.24":   "LED 3B|PWM Low Power|Pinscape AIO|Small LED 3B|aioBoardPWMOutputSelector",
        "3.25":   "LED 4R|PWM Low Power|Pinscape AIO|Small LED 4R|aioBoardPWMOutputSelector",
        "3.26":   "LED 4G|PWM Low Power|Pinscape AIO|Small LED 4G|aioBoardPWMOutputSelector",
        "3.27":   "LED 4B|PWM Low Power|Pinscape AIO|Small LED 4B|aioBoardPWMOutputSelector",
        "3.28":   "LED 5R|PWM Low Power|Pinscape AIO|Small LED 5R|aioBoardPWMOutputSelector",
        "3.29":   "LED 5G|PWM Low Power|Pinscape AIO|Small LED 5G|aioBoardPWMOutputSelector",
        "3.30":   "LED 5B|PWM Low Power|Pinscape AIO|Small LED 5B|aioBoardPWMOutputSelector",
        "3.31":   "LED 6|PWM Low Power|Pinscape AIO|Small LED 6|aioBoardPWMOutputSelector",

        "3.32":   "Output 1|PWM Hi Power|Pinscape AIO|Power 1|aioBoardPWMOutputSelector",
        "3.33":   "Output 2|PWM Hi Power|Pinscape AIO|Power 2|aioBoardPWMOutputSelector",
        "3.34":   "Output 3|PWM Hi Power|Pinscape AIO|Power 3|aioBoardPWMOutputSelector",
        "3.35":   "Output 4|PWM Hi Power|Pinscape AIO|Power 4|aioBoardPWMOutputSelector",
        "3.36":   "Output 5|PWM Hi Power|Pinscape AIO|Power 5|aioBoardPWMOutputSelector",
        "3.37":   "Output 6|PWM Hi Power|Pinscape AIO|Power 6|aioBoardPWMOutputSelector",
        "3.38":   "Output 7|PWM Hi Power|Pinscape AIO|Power 7|aioBoardPWMOutputSelector",
        "3.39":   "Output 8|PWM Hi Power|Pinscape AIO|Power 8|aioBoardPWMOutputSelector",
        "3.40":   "Output 9|PWM Hi Power|Pinscape AIO|Power 9|aioBoardPWMOutputSelector",
        "3.41":   "Output 10|PWM Hi Power|Pinscape AIO|Power 10|aioBoardPWMOutputSelector",
        "3.42":   "Output 11|PWM Hi Power|Pinscape AIO|Power 11|aioBoardPWMOutputSelector",
        "3.43":   "Output 12|PWM Hi Power|Pinscape AIO|Power 12|aioBoardPWMOutputSelector",
        "3.44":   "Output 13|PWM Hi Power|Pinscape AIO|Power 13|aioBoardPWMOutputSelector",
        "3.45":   "Output 14|PWM Hi Power|Pinscape AIO|Power 14|aioBoardPWMOutputSelector",
        "3.46":   "Output 15|PWM Hi Power|Pinscape AIO|Power 15|aioBoardPWMOutputSelector",
        "3.47":   "Output 16|PWM Hi Power|Pinscape AIO|Power 16|aioBoardPWMOutputSelector",

        "3.48":   "Output 17|PWM Hi Power|Pinscape AIO|Power 17|aioBoardPWMOutputSelector",
        "3.49":   "Output 18|PWM Hi Power|Pinscape AIO|Power 18|aioBoardPWMOutputSelector",
        "3.50":   "Output 19|PWM Hi Power|Pinscape AIO|Power 19|aioBoardPWMOutputSelector",
        "3.51":   "Output 20|PWM Hi Power|Pinscape AIO|Power 20|aioBoardPWMOutputSelector",
        "3.52":   "Output 21|PWM Hi Power|Pinscape AIO|Power 21|aioBoardPWMOutputSelector",
        "3.53":   "Output 22|PWM Hi Power|Pinscape AIO|Power 22|aioBoardPWMOutputSelector",
        "3.54":   "Output 23|PWM Hi Power|Pinscape AIO|Power 23|aioBoardPWMOutputSelector",
        "3.55":   "Output 24|PWM Hi Power|Pinscape AIO|Power 24|aioBoardPWMOutputSelector",
        "3.56":   "Output 25|PWM Hi Power|Pinscape AIO|Power 25|aioBoardPWMOutputSelector",
        "3.57":   "Output 26|PWM Hi Power|Pinscape AIO|Power 26|aioBoardPWMOutputSelector",
        "3.58":   "Output 27|PWM Hi Power|Pinscape AIO|Power 27|aioBoardPWMOutputSelector",
        "3.59":   "Output 28|PWM Hi Power|Pinscape AIO|Power 28|aioBoardPWMOutputSelector",
        "3.60":   "Output 29|PWM Hi Power|Pinscape AIO|Power 29|aioBoardPWMOutputSelector",
        "3.61":   "Output 30|PWM Hi Power|Pinscape AIO|Power 30|aioBoardPWMOutputSelector",
        "3.62":   "Output 31|PWM Hi Power|Pinscape AIO|Power 31|aioBoardPWMOutputSelector",
        "3.63":   "Output 32|PWM Hi Power|Pinscape AIO|Power 32|aioBoardPWMOutputSelector",

        "4.0":    "Output 1|Timed Digital|Pinscape AIO|Chime 1|aioBoardDigitalOutputSelector",
        "4.1":    "Output 2|Timed Digital|Pinscape AIO|Chime 2|aioBoardDigitalOutputSelector",
        "4.2":    "Output 3|Timed Digital|Pinscape AIO|Chime 3|aioBoardDigitalOutputSelector",
        "4.3":    "Output 4|Timed Digital|Pinscape AIO|Chime 4|aioBoardDigitalOutputSelector",
        "4.4":    "Output 5|Timed Digital|Pinscape AIO|Chime 5|aioBoardDigitalOutputSelector",
        "4.5":    "Output 6|Timed Digital|Pinscape AIO|Chime 6|aioBoardDigitalOutputSelector",
        "4.6":    "Output 7|Timed Digital|Pinscape AIO|Chime 7|aioBoardDigitalOutputSelector",
        "4.7":    "Output 8|Timed Digital|Pinscape AIO|Chime 8|aioBoardDigitalOutputSelector"
    };

    // add four power boards worth of outputs
    for (var i = 0 ; i < 128 ; ++i) {
        o["3." + (i+64)] =
            "Output " + ((i%32) + 1)
            + "|PWM Hi Power"
            + "|Power Board " + (Math.floor(i/32) + 1)
            + "|JP" + (Math.floor((i%32)/16)+5)+ "-" + ((i%16)+1)
            + "|aioPowerBoardOutputSelector";
    }

    // add four chime boards worth of outputs
    for (i = 0 ; i < 32 ; ++i) {
        o["4." + (i+8)] =
            "Output " + ((i%8) + 1)
            + "|Timed Digital"
            + "|Chime Board " + (Math.floor(i/8) + 1)
            + "|JP9-" + ((i%8)+1)
            + "|aioChimeBoardOutputSelector";
    }

    // Table of all internal GPIO connections on the main expansion board.
    // These generally can't be used for any other purpose.
    var gInternal = {
        "PTC10":  "TLC5940 XLAT",
        "PTC6":   "TLC5940 SIN",
        "PTC5":   "TLC5940 SCLK",
        "PTC7":   "TLC5940 BLANK",
        "PTA1":   "TLC5940 GSCLK",
        "PTD4":   "74HC595 ENA",
        "PTA4":   "74HC595 SCLK",
        "PTA5":   "74HC595 SOUT",
        "PTA12":  "74HC595 LATCH",
        "PTC9":   "IR OUT",
        "PTA13":  "IR IN",
        "PTD2":   "PSU2 STATUS",
        "PTD3":   "TV RELAY",
        "PTE0":   "PSU2 LATCH"
    };

    // Table of all external GPIO connections on the main expansion
    // board.  These KL25Z pins are connected more or less directly
    // to external headers on the board, so they can be re-purposed
    // (sometimes with restrictions) for other external uses.  For
    // example, if a plunger sensor isn't being used, all of the
    // plunger input pins can be used as button inputs instead.
    var gExternal = {
        "PTE23":  "Calibration LED-|LED-",
        "PTE29":  "Calibration A|A",
        "PTE22":  "Plunger CHB|CHB",
        "PTD5":   "Plunger CHB|CHB",
        "PTE21":  "Plunger CHA/SCL|CHA/SCL",
        "PTD0":   "Plunger CHA/SCL|CHA/SCL",
        "PTE20":  "Plunger SDA|SDA",
        "PTB0":   "Plunger Wiper/INT|Wiper/INT",
        "PTC8":   "Knocker|Knocker|Timed Digital",
        "PTC4":   "Expansion C4|C4",
        "PTC3":   "Expansion C3|C3",
        "PTC0":   "Expansion C0|C0",
        "PTA2":   "Expansion A2|A2"
     };

    // Build a combined table of the GPIO port aliases for internal and
    // external Pinscape AIO board connections.
    var g = { };
    $.each(gInternal, function(k, v) { g[k] = v + "|Internal"; });
    $.each(gExternal, function(k, v) { g[k] = v.split("|").slice(0, 2).join("|"); });

    // Add GPIO aliases for all of the buttons
    $.each(pinscapeAIOFactoryConfig.buttons, function(k, v) {
        g[v.pin] = "Button " + k + "|Digital In|Pinscape AIO|Button Inputs-" + k + "|aioBoardInputSelector";
    });

    // Add all of the external ports as output aliases.
    $.each(gExternal, function(k, v)
    {
        // break v into fields - Descriptive Name, Jumper, [output type description]
        v = v.split("|");

        // if it's a PWM-capable port, add a PWM output type for it
        var gp = gpioPinsByName[k] || { };
        if (gp.pwm)
            o["1." + k] = [v[0], v[2] || "PWM GPIO", "Pinscape AIO", v[1], "aioBoardPWMOutputSelector"].join("|");

        // add a Digital Out type for it
        o["2." + k] = [v[0], v[2] || "Digital GPIO", "Pinscape AIO", v[1], "aioBoardDigitalOutputSelector"].join("|");
    });

    // remember these
    aioOutPortAlias = o;
    aioGpioPortAlias = g;
})();

var liteOutPortAlias, liteGpioPortAlias;
// build the Pinscape Lite out port alias table
(function () {
    var o = {
        "3.0": "LED 1|PWM Low Power|Pinscape Lite|Small LED 1|liteBoardPWMOutputSelector",
        "3.1": "LED 2|PWM Low Power|Pinscape Lite|Small LED 2|liteBoardPWMOutputSelector",
        "3.2": "LED 3|PWM Low Power|Pinscape Lite|Small LED 3|liteBoardPWMOutputSelector",
        "3.3": "LED 4|PWM Low Power|Pinscape Lite|Small LED 4|liteBoardPWMOutputSelector",
        "3.4": "LED 5|PWM Low Power|Pinscape Lite|Small LED 5|liteBoardPWMOutputSelector",
        "3.5": "LED 6|PWM Low Power|Pinscape Lite|Small LED 6|liteBoardPWMOutputSelector",
        "3.6": "LED 7|PWM Low Power|Pinscape Lite|Small LED 7|liteBoardPWMOutputSelector",
        "3.7": "LED 8|PWM Low Power|Pinscape Lite|Small LED 8|liteBoardPWMOutputSelector",
        "3.8": "LED 9|PWM Low Power|Pinscape Lite|Small LED 9|liteBoardPWMOutputSelector",
        "3.9": "LED 10|PWM Low Power|Pinscape Lite|Small LED 10|liteBoardPWMOutputSelector",
        "3.10": "LED 11|PWM Low Power|Pinscape Lite|Small LED 11|liteBoardPWMOutputSelector",
        "3.11": "LED 12|PWM Low Power|Pinscape Lite|Small LED 12|liteBoardPWMOutputSelector",
        "3.12": "LED 13|PWM Low Power|Pinscape Lite|Small LED 13|liteBoardPWMOutputSelector",
        "3.13": "LED 14|PWM Low Power|Pinscape Lite|Small LED 14|liteBoardPWMOutputSelector",
        "3.14": "LED 15|PWM Low Power|Pinscape Lite|Small LED 15|liteBoardPWMOutputSelector",
        "3.15": "LED 16|PWM Low Power|Pinscape Lite|Small LED 16|liteBoardPWMOutputSelector",
    };

    // add four power boards worth of outputs
    for (var i = 0; i < 128; ++i) {
        o["3." + (i + 16)] =
            "Output " + ((i % 32) + 1)
            + "|PWM Hi Power"
            + "|Power Board " + (Math.floor(i / 32) + 1)
            + "|JP" + (Math.floor((i % 32) / 16) + 5) + "-" + ((i % 16) + 1)
            + "|litePowerBoardOutputSelector";
    }

    // add four chime boards worth of outputs
    for (i = 0; i < 32; ++i) {
        o["4." + i] =
            "Output " + ((i % 8) + 1)
            + "|Timed Digital"
            + "|Chime Board " + (Math.floor(i / 8) + 1)
            + "|JP9-" + ((i % 8) + 1)
            + "|chimeBoardOutputSelector";
    }

    // Table of all internal GPIO connections on the main expansion board.
    // These generally can't be used for any other purpose.
    var gInternal = {
        "PTC10": "TLC5940 XLAT",
        "PTC6": "TLC5940 SIN",
        "PTC5": "TLC5940 SCLK",
        "PTC7": "TLC5940 BLANK",
        "PTA1": "TLC5940 GSCLK",
        "PTD4": "74HC595 ENA",
        "PTA4": "74HC595 SCLK",
        "PTA5": "74HC595 SOUT",
        "PTA12": "74HC595 LATCH",
    };

    // Table of all external GPIO connections on the main expansion
    // board.  These KL25Z pins are connected more or less directly
    // to external headers on the board, so they can be re-purposed
    // (sometimes with restrictions) for other external uses.  For
    // example, if a plunger sensor isn't being used, all of the
    // plunger input pins can be used as button inputs instead.
    var gExternal = {
        "PTE22": "Plunger CHB|CHB",
        "PTD5": "Plunger CHB|CHB",
        "PTE21": "Plunger CHA/SCL|CHA/SCL",
        "PTD0": "Plunger CHA/SCL|CHA/SCL",
        "PTE20": "Plunger SDA|SDA",
        "PTB0": "Plunger Wiper/INT|Wiper/INT",
        "PTA2": "Power 1|Power 1|PWM GPIO",
        "PTA13": "Power 2|Power 2|PWM GPIO",
        "PTD2": "Power 3|Power 3|Digital GPIO",
        "PTD3": "Power 4|Power 4|Digital GPIO",
        "PTC8": "Power 5|Power 5|Digital GPIO",
        "PTC9": "Power 6|Power 6|Digital GPIO",
        "PTC0": "Power 7|Power 7|Digital GPIO",
        "PTC3": "Power 8|Power 8|Digital GPIO",
        "PTC4": "Power 9|Power 9|Digital GPIO",
        "PTE23": "Power 10|Power 10|Digital GPIO",
        "PTE29": "Power 11|Power 11|Digital GPIO",
        "PTE0": "Power 12|Power 12|Digital GPIO"
    };

    // Build a combined table of the GPIO port aliases for internal and
    // external Pinscape Lite board connections.
    var g = {};
    $.each(gInternal, function (k, v) { g[k] = v + "|Internal"; });
    $.each(gExternal, function (k, v) { g[k] = v.split("|").slice(0, 2).join("|"); });

    // Add GPIO aliases for all of the buttons
    $.each(pinscapeLiteFactoryConfig.buttons, function (k, v) {
        g[v.pin] = "Button " + k + "|Digital In|Pinscape Lite|Button Inputs-" + k + "|liteBoardInputSelector";
    });

    // Add all of the external ports as output aliases.
    $.each(gExternal, function (k, v) {
        // break v into fields - Descriptive Name, Jumper, [output type description]
        v = v.split("|");

        // if it's a PWM-capable port, add a PWM output type for it
        var gp = gpioPinsByName[k] || {};
        if (gp.pwm)
            o["1." + k] = [v[0], v[2] || "PWM GPIO", "Pinscape Lite", v[1], "liteBoardPWMOutputSelector"].join("|");

        // add a Digital Out type for it
        o["2." + k] = [v[0], v[2] || "Digital GPIO", "Pinscape Lite", v[1], "liteBoardDigitalOutputSelector"].join("|");
    });

    // remember these
    liteOutPortAlias = o;
    liteGpioPortAlias = g;
})();

// build the Pinscape RigMaster out port alias table
var rigMasterOutPortAlias, rigMasterGpioPortAlias;
(function () {
    var o = {
    };

    // add seven Mollusk boards worth of outputs
    for (var i = 0; i < 128; ++i) {
        o["3." + (i)] =
            "Output " + ((i % 16) + 1)
            + "|Mollusk PWM out"
            + "|Mollusk " + (Math.floor(i / 16) + 1)
            + "|Mollusk" + (Math.floor(i / 16) + 1) + " Out " + ((i % 16) + 1)
            + "|molluskBoardOutputSelector";
    }
    // Table of all internal GPIO connections on the main expansion board.
    // These generally can't be used for any other purpose.
    var gInternal = {
        "PTC10": "TLC5940 XLAT",
        "PTC6": "TLC5940 SIN",
        "PTC5": "TLC5940 SCLK",
        "PTC11": "TLC5940 BLANK",
        "PTA13": "TLC5940 GSCLK",
    };

    // Table of all external GPIO connections on the main expansion
    // board.  These KL25Z pins are connected more or less directly
    // to external headers on the board, so they can be re-purposed
    // (sometimes with restrictions) for other external uses.  For
    // example, if a plunger sensor isn't being used, all of the
    // plunger input pins can be used as button inputs instead.
    var gExternal = {
        "PTB0": "Plunger Wiper/INT|Wiper/INT",
        "PTB1": "Power 1|Power 1|Digital GPIO",
        "PTB2": "Power 2|Power 2|Digital GPIO",
        "PTB3": "Power 3|Power 3|Digital GPIO",
        "PTB8": "Power 4|Power 4|Digital GPIO",
        "PTB9": "Power 5|Power 5|Digital GPIO",
        "PTB10": "Power 6|Power 6|Digital GPIO",
        "PTB11": "Power 7|Power 7|Digital GPIO",
        "PTA17": "Power 8|Power 8|Digital GPIO",
        "PTA1": "Power 9|Power 9|PWM GPIO",
        "PTA2": "Power 10|Power 10|PWM GPIO",
        "PTA4": "Power 11|Power 11|PWM GPIO",
        "PTA5": "Power 12|Power 12|PWM GPIO",
        "PTD0": "Power 13|Power 13|PWM GPIO",
        "PTC4": "Power 14|Power 14|PWM GPIO",
        "PTC8": "Power 15|Power 15|PWM GPIO",
        "PTC9": "Power 16|Power 16|PWM GPIO"
    };

    // Build a combined table of the GPIO port aliases for internal and
    // external Pinscape Lite board connections.
    var g = {};
    $.each(gInternal, function (k, v) { g[k] = v + "|Internal"; });
    $.each(gExternal, function (k, v) { g[k] = v.split("|").slice(0, 2).join("|"); });

    // Add GPIO aliases for all of the buttons
    $.each(rigMasterFactoryConfig.buttons, function (k, v) {
        g[v.pin] = "Button " + k + "|Digital In|RigMaster|Button Inputs-" + k + "|rigMasterBoardInputSelector";
    });

    // Add all of the external ports as output aliases.
    $.each(gExternal, function (k, v) {
        // break v into fields - Descriptive Name, Jumper, [output type description]
        v = v.split("|");

        // if it's a PWM-capable port, add a PWM output type for it
        var gp = gpioPinsByName[k] || {};
        if (gp.pwm)
            o["1." + k] = [v[0], v[2] || "PWM GPIO", "RigMaster", v[1], "rigMasterBoardPWMOutputSelector"].join("|");

        // add a Digital Out type for it
        o["2." + k] = [v[0], v[2] || "Digital GPIO", "RigMaster", v[1], "rigMasterBoardDigitalOutputSelector"].join("|");
    });

    // remember these
    rigMasterOutPortAlias = o;
    rigMasterGpioPortAlias = g;
})();

var klShieldOutPortAlias, klShieldGpioPortAlias;
// build the Pinscape KLShield out port alias table
(function () {
    var o = {
    };

    // add eight Mollusk boards worth of outputs
    for (var i = 0; i < 128; ++i) {
        o["3." + (i)] =
            "Output " + ((i % 16) + 1)
            + "|Mollusk PWM out"
            + "|Mollusk " + (Math.floor(i / 16) + 1)
            + "|Mollusk" + (Math.floor(i / 16) + 1) + " Out " + ((i % 16) + 1)
            + "|molluskBoardOutputSelector";
    }
    // Table of all internal GPIO connections on the main expansion board.
    // These generally can't be used for any other purpose.
    var gInternal = {
        "PTC10": "TLC5940 XLAT",
        "PTC6": "TLC5940 SIN",
        "PTC5": "TLC5940 SCLK",
        "PTC11": "TLC5940 BLANK",
        "PTA13": "TLC5940 GSCLK",
        "PTB0": "Plunger Wiper/INT|Wiper/INT",

    };

    // Table of all external GPIO connections on the main expansion
    // board.  These KL25Z pins are connected more or less directly
    // to external headers on the board, so they can be re-purposed
    // (sometimes with restrictions) for other external uses.  For
    // example, if a plunger sensor isn't being used, all of the
    // plunger input pins can be used as button inputs instead.
    var gExternal = {
        "PTC13": "Cal Button LED|PLUNGER",
        "PTC12": "Cal Button Switch|PLUNGER",
        "PTE30": "PTE30|BOUTON  1",
        "PTE29": "PTE29|BOUTON 1",
        "PTE23": "PTE23|BOUTON 1",
        "PTE22": "PTE22|BOUTON 1",
        "PTE21": "PTE21|BOUTON 1",
        "PTE20": "PTE20|BOUTON 1",
        "PTE5": "PTE5|BOUTON 1",
        "PTE4": "PTE4|BOUTON 1",
        "PTE3": "PTE3|BOUTON 1",
        "PTE2": "PTE2|BOUTON 1",
        "PTC16": "PTC16|BOUTON 2",
        "PTC17": "PTC17|BOUTON 2",
        "PTC7": "PTC7|BOUTON 2",
        "PTD2": "PTD2|BOUTON 2",
        "PTD3": "PTD3|BOUTON 2",
        "PTD4": "PTD4|BOUTON 2",
        "PTD5": "PTD5|BOUTON 2",
        "PTD64": "PTD6|BOUTON 2",
        "PTD7": "PTD7|BOUTON 2",
        "PTE0": "PTE0|BOUTON 2",
        "PTE1": "PTE1|BOUTON 2",
    };

    // Build a combined table of the GPIO port aliases for internal and
    // external Pinscape Lite board connections.
    var g = {};
    $.each(gInternal, function (k, v) { g[k] = v + "|Internal"; });
    $.each(gExternal, function (k, v) { g[k] = v.split("|").slice(0, 2).join("|"); });

    // Add GPIO aliases for all of the buttons
    $.each(klShieldFactoryConfig.buttons, function (k, v) {
        g[v.pin] = "Button " + k + "|KLShield BUTTON|KLShield|Button Inputs-" + k + "|klShieldBoardInputSelector";
    });

    // Add all of the external ports as output aliases.
    $.each(gExternal, function (k, v) {
        // break v into fields - Descriptive Name, Jumper, [output type description]
        v = v.split("|");

        // if it's a PWM-capable port, add a PWM output type for it
        var gp = gpioPinsByName[k] || {};
        if (gp.pwm)
            o["1." + k] = [v[0], v[2] || "PWM GPIO", "KLShield", v[1], "klShieldBoardPWMOutputSelector"].join("|");

        // add a Digital Out type for it
        o["2." + k] = [v[0], v[2] || "Digital GPIO", "KLShield", v[1], "klShieldBoardDigitalOutputSelector"].join("|");
    });

    // remember these
    klShieldOutPortAlias = o;
    klShieldGpioPortAlias = g;
})();

// Build a master map of pin to header info mappings for the given
// system configuration.  The system type is the same as in the USB
// protocol for the system type (config variable 14).
//
// The results look like this:
//
//  <returned map>[pinType + "." + pinId] = {
//     board: "board/chip name",  // kl25z, tlc5940, 74hc595, tlc59116, expansion main, expansion power, expansion chime
//     header: "header name",     // JP1, etc; undefined for chips
//     pinNum: n,                 // pin number on header or chip
//     image: "image name.png",   // header pin diagram image file
//     wrapper: "class",          // wrapper class for pin layout image (kl25PinSelector, tlc5940PinSelector, ...)
//     x: x,                      // x coordinate of pin in diagram
//     y: y                       // y coordinate of pin in diagram
//  }
function buildPinToHeaderMap(sysType)
{
    var pinInfoMap = { };
    switch (sysType)
    {
    case 0:
        //
        // standalone KL2Z
        //

        // Populate the KL25Z pins.  For PWM-capable pins, index them
        // by both 1.x and 2.x keys, since those pins can be used both
        // ways.
        $.each(kl25z_headers, function(hdrName, hdrInfo) {
            forEachPin(hdrInfo, function(pin, n, x, y) {
                var o = {
                    board: "kl25z",
                    header: hdrName,
                    pinNum: n,
                    image: "kl25zPins.png",
                    wrapper: "kl25zPinSelector",
                    x: x,
                    y: y
                };
                pinInfoMap["2." + pin] = o; // digital out - all ports eligible
                var g = gpioPinsByName[pin];
                if (g && g.pwm)
                    pinInfoMap["1." + pin] = o; // pwm out
            });
        });

        // Populate TLC5940, 74HC595, and TLC59116 pins.  Allow for
        // eight copies of each board.
        $.each({
            "tlc5940":  { pins: tlc5940_pins, nPins: 16, image: "tlc5940Pins.png", wrapper: "tlc5940PinSelector", typePrefix: "3." },
            "74hc595":  { pins: hc595_pins, nPins: 8, image: "74hc595Pins.png", wrapper: "hc595PinSelector", typePrefix: "4." },
            "tlc59116": { pins: tlc59116_pins, nPins: 16, image: "tlc59116Pins.png", wrapper: "tlc59116PinSelector", typePrefix: "6." },
        }, function(chipName, chipInfo) {
            forEachPin(chipInfo.pins, function(pin, n, x, y) {
                // all of the chips use "OUTn" as the pin name for output pins
                if (/OUT(\d+)/.test(pin)) {
                    // infer the pin number from the OUTn name
                    var n = +RegExp.$1;

                    // set up eight copies of the chip
                    for (var i = 1; i <= 8; ++i, n += chipInfo.nPins) {
                        pinInfoMap[chipInfo.typePrefix + n] = {
                            board: chipName + " #" + i,
                            pinNum: pin,
                            image: chipInfo.image,
                            wrapper: chipInfo.wrapper,
                            x: x,
                            y: y
                        };
                    }
                }
            });
        });
        break;

    case 1:
        // Pinscape expansion boards.  Populate each board, allowing
        // for up to four copies of each secondary board.
        $.each({
            "expansion main": { headers: mainBoard_headers, image: "mainBoardPins.png", copies: 1 },
            "expansion power": { headers: powerBoard_headers, image: "powerBoardPins.png", copies: 4 },
            "expansion chime": { headers: chimeBoard_headers, image: "chimeBoardPins.png", copies: 4 },
        }, function(boardName, boardInfo) {
            $.each(boardInfo.headers, function(headerName, headerInfo) {
                forEachPin(headerInfo, function(pinName, n, x, y) {
                    for (var i = 1; i <= boardInfo.copies; ++i) {
                        var o = {
                            board: boardName + (boardInfo.copies > 1 ? " #" + i : ""),
                            header: headerName,
                            pinNum: n,
                            image: boardInfo.image,
                            wrapper: "expBoardPinSelector",
                            x: x,
                            y: y
                        };

                        // for PTxx ports, index them under 2.x (digital out) and
                        // 1.x (PWM out), as applicable
                        if (/PT[A-E]\d+/.test(pinName)) {
                            // all ports can be used as digital outs
                            pinInfoMap["2." + pinName] = o;

                            // check for PWM capability
                            var g = gpioPinsByName[pinName];
                            if (g && g.pwm)
                                pinInfoMap["1." + pinName] = o;
                        }
                        else {
                            // not a GPIO port - use the normal N.M notation
                            pinInfoMap[pinName] = o;
                        }
                    }
                });
            });
        });
        break;

    case 2:
        // Pinscape AIO board.  Populate each board, allowing
        // for up to four copies of each secondary board.
        $.each({
            "expansion main": { headers: aioBoard_headers, image: "aioBoardPins.png", copies: 1 },
            "expansion power": { headers: aio_powerBoard_headers, image: "powerBoardPins.png", copies: 4 },
            "expansion chime": { headers: aio_chimeBoard_headers, image: "chimeBoardPins.png", copies: 4 },
        }, function(boardName, boardInfo) {
            $.each(boardInfo.headers, function(headerName, headerInfo) {
                forEachPin(headerInfo, function(pinName, n, x, y) {
                    for (var i = 1; i <= boardInfo.copies; ++i) {
                        var o = {
                            board: boardName + (boardInfo.copies > 1 ? " #" + i : ""),
                            header: headerName,
                            pinNum: n,
                            image: boardInfo.image,
                            wrapper: "aioBoardPinSelector",
                            x: x,
                            y: y
                        };

                        // for PTxx ports, index them under 2.x (digital out) and
                        // 1.x (PWM out), as applicable
                        if (/PT[A-E]\d+/.test(pinName)) {
                            // all ports can be used as digital outs
                            pinInfoMap["2." + pinName] = o;

                            // check for PWM capability
                            var g = gpioPinsByName[pinName];
                            if (g && g.pwm)
                                pinInfoMap["1." + pinName] = o;
                        }
                        else {
                            // not a GPIO port - use the normal N.M notation
                            pinInfoMap[pinName] = o;
                        }
                    }
                });
            });
        });
        break;

    case 3:
       // Pinscape Lite board.  Populate each board, allowing
        // for up to four copies of each secondary board.
        $.each({
            "expansion main": { headers: liteBoard_headers, image: "liteBoardPins.png", copies: 1 },
            "expansion power": { headers: lite_powerBoard_headers, image: "powerBoardPins.png", copies: 4 },
            "expansion chime": { headers: lite_chimeBoard_headers, image: "chimeBoardPins.png", copies: 4 },
        }, function (boardName, boardInfo) {
            $.each(boardInfo.headers, function (headerName, headerInfo) {
                forEachPin(headerInfo, function (pinName, n, x, y) {
                    for (var i = 1; i <= boardInfo.copies; ++i) {
                        var o = {
                            board: boardName + (boardInfo.copies > 1 ? " #" + i : ""),
                            header: headerName,
                            pinNum: n,
                            image: boardInfo.image,
                            wrapper: "liteBoardPinSelector",
                            x: x,
                            y: y
                        };

                        // for PTxx ports, index them under 2.x (digital out) and
                        // 1.x (PWM out), as applicable
                        if (/PT[A-E]\d+/.test(pinName)) {
                            // all ports can be used as digital outs
                            pinInfoMap["2." + pinName] = o;

                            // check for PWM capability
                            var g = gpioPinsByName[pinName];
                            if (g && g.pwm)
                                pinInfoMap["1." + pinName] = o;
                        }
                        else {
                            // not a GPIO port - use the normal N.M notation
                            pinInfoMap[pinName] = o;
                        }
                    }
                });
            });
        });
        break;

    case 4:
        // Arnoz RigMaster.  Populate each board, allowing
        // for up to seven copies of Mollusk board.
        $.each({
            "expansion main": { headers: rigMasterBoard_headers, image: "RigMasterBoardPins.png", copies: 1 },
            "expansion mollusk": { headers: molluskBoard_headers, image: "MolluskBoardPins.png", copies: 7 },
        }, function (boardName, boardInfo) {
            $.each(boardInfo.headers, function (headerName, headerInfo) {
                forEachPin(headerInfo, function (pinName, n, x, y) {
                    for (var i = 1; i <= boardInfo.copies; ++i) {
                        var o = {
                            board: boardName + (boardInfo.copies > 1 ? " #" + i : ""),
                            header: headerName,
                            pinNum: n,
                            image: boardInfo.image,
                            wrapper: "rigMasterBoardPinSelector",
                            x: x,
                            y: y
                        };

                        // for PTxx ports, index them under 2.x (digital out) and
                        // 1.x (PWM out), as applicable
                        if (/PT[A-E]\d+/.test(pinName)) {
                            // all ports can be used as digital outs
                            pinInfoMap["2." + pinName] = o;

                            // check for PWM capability
                            var g = gpioPinsByName[pinName];
                            if (g && g.pwm)
                                pinInfoMap["1." + pinName] = o;
                        }
                        else {
                            // not a GPIO port - use the normal N.M notation
                            pinInfoMap[pinName] = o;
                        }
                    }
                });
            });
        });
        break;

    case 5:
        // Arnoz KLShield.  Populate each board, allowing
        // for up to seven copies of Mollusk board.
        $.each({
            "expansion main": { headers: klShieldBoard_headers, image: "KLShieldBoardPins.png", copies: 1 },
            "expansion mollusk": { headers: molluskBoard_headers, image: "MolluskBoardPins.png", copies: 8 },
        }, function (boardName, boardInfo) {
            $.each(boardInfo.headers, function (headerName, headerInfo) {
                forEachPin(headerInfo, function (pinName, n, x, y) {
                    for (var i = 1; i <= boardInfo.copies; ++i) {
                        var o = {
                            board: boardName + (boardInfo.copies > 1 ? " #" + i : ""),
                            header: headerName,
                            pinNum: n,
                            image: boardInfo.image,
                            wrapper: "klShieldBoardPinSelector",
                            x: x,
                            y: y
                        };
                        
                        // for PTxx ports, index them under 2.x (digital out) and
                        // 1.x (PWM out), as applicable
                        if (/PT[A-E]\d+/.test(pinName)) {
                            // all ports can be used as digital outs
                            pinInfoMap["2." + pinName] = o;

                            // check for PWM capability
                            var g = gpioPinsByName[pinName];
                            if (g && g.pwm)
                                pinInfoMap["1." + pinName] = o;
                        }
                        else {
                            // not a GPIO port - use the normal N.M notation
                            pinInfoMap[pinName] = o;
                        }
                    }
                });
            });
        });
        break;
    }

    // return the result
    return pinInfoMap;
}

 