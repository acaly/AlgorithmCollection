ReciprocalFibonacci
=======

Given an accuracy, calculation of the [reciprocal fibonacci constant (ψ)](https://en.wikipedia.org/wiki/Reciprocal_Fibonacci_constant). 
Algorithms here can handle very big input using BigInteger struct. The number is calculated digit by digit so you 
can run it with a very large input and stop at any where you want.

Time and memory required for each methods
-------
```
Testing first 5000 digits
1 digits
...Calculation finishes for 5000. Time = 271045 ms. Memory = 49.04 MB.
2 digits
...Calculation finishes for 5000. Time = 136400 ms. Memory = 49.08 MB.
16 digits
...Calculation finishes for 5000. Time = 29475 ms. Memory = 49.63 MB.
50 digits
...Calculation finishes for 5000. Time = 21755 ms. Memory = 50.76 MB.
100 digits
...Calculation finishes for 5000. Time = 19885 ms. Memory = 52.73 MB.
100 digits merge
...Calculation finishes for 5000. Time = 15619 ms. Memory = 39.65 MB.
Dynamic merge
...Calculation finishes for 5000. Time = 16437 ms. Memory = 40.51 MB.


Testing first 500 digits
1 digits
...Calculation finishes for 500. Time = 410 ms. Memory = 0.65 MB.
2 digits
...Calculation finishes for 500. Time = 289 ms. Memory = 0.66 MB.
16 digits
...Calculation finishes for 500. Time = 67 ms. Memory = 0.73 MB.
50 digits
...Calculation finishes for 500. Time = 50 ms. Memory = 0.86 MB.
100 digits
...Calculation finishes for 500. Time = 62 ms. Memory = 1.12 MB.
100 digits merge
...Calculation finishes for 500. Time = 54 ms. Memory = 0.85 MB.
Dynamic merge
...Calculation finishes for 500. Time = 37 ms. Memory = 0.57 MB.
```
