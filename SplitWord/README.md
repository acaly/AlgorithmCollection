SplitWord
=======

Given dictionary (a list of vocabulary) and a message that contains words in the dictionary but without space, 
split the message into words. Should indicate failure if there's no solution.

Without ```.```, ```*``` and ```+```, this is a simplified version of regex matching. If size of each word in 
the dictionary is a constant, the algorithm runs in O(M log M + N) time, where M = number of words in dictionary, 
N = len of the message.
