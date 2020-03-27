\ Alternate implementation without DMA, works on any GPIO
\ because of implizit timing via nop it will only work on stm32f103 @ 72MHz
\
\ this only works from ram. flash is to slow?


[ifndef] MAX-LEDS 15 constant MAX-LEDS [then]
[ifndef] LEDS PB12 constant LEDS [then]

0 variable ledshow 



: led2bytes ( u - u ) \ 3 bytes per pixel
  3 *
;

MAX-LEDS led2bytes buffer: strip

\ external api

: setpixel ( r g b index - )
    3 * 2+ strip +
    tuck c!
    1- tuck c!
    1- c!
;

: led-clear ( - )
    strip MAX-LEDS led2bytes $00 fill
;



\ works only if running from ram
\ : led-show
\    LEDS ioc! 1 ms
\    MAX-LEDS 3 * 0 do
\	strip i + c@
\	8 0 do
\	    dup $80 i rshift and
\	    if
\		LEDS dup
\		ios!
\		nop nop nop
\		ioc!
\	    else
\		LEDS dup
\		ios!
\		ioc!
\		nop nop nop
\	    then
\	loop
\	drop
\   loop
\ ;


\ dispatching to generated ram word
: led-show
    ledshow @
    dup 0<> if
	execute
    else
	CR ." call led-init first."
    then
;



\ generating a ram-word for led-show
: led-init
    ledshow @ 0=
    if
	s" : led-show LEDS ioc! 1 ms MAX-LEDS 3 * 0 do strip i + c@ 8 0 do dup $80 i rshift and if LEDS dup ios! nop nop nop ioc! else LEDS dup	ios! ioc! nop nop nop then loop	drop loop ; "
	evaluate
	s" ['] led-show" evaluate
	ledshow ! 
    then
    led-clear
    omode-pp LEDS io-mode!
    LEDS ioc! 50 ms
    led-show
;

\

\ Output memory buffer
: led. ( - )
  MAX-LEDS 0 do
    CR i h.2 ." :"
    3 0 do space strip j 3 * + i + c@ h.2 loop
  loop
;

\ returns one of the colors for use in an animation
: colorwheel ( i - u u u )
  5 mod case
    0 of $ff $ff $ff endof
    1 of $ff $00 $00 endof
    2 of $00 $ff $00 endof
    3 of $00 $00 $ff endof
    4 of $00 $00 $00 endof
  endcase ;

\ output some demo data ( input is offset for animation)
: demodata ( i )
  MAX-LEDS 0 do
    dup i + colorwheel i setpixel
  loop led-show
;

: ringanimate ( - )
  100 0  do
      i demodata drop 200 ms
  loop
;


