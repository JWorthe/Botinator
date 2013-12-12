Botinator
=========

My entry for the Entelect 100k Challenge 2013.

The task for the 2013 100k challenge was to write a program which connects to a SOAP web service and controls the tanks in a game of Battle City. Each player has two tanks and a base on a two dimensional board. The player who manages to destroy their opponent's base first wins.

I'm currently employed by Entelect, so I could not enter the 100k challenge, but we did have an internal version of the challenge. This bot came second in the internal challenge.

This is an intentionally 'stupid' bot. It doesn't use any fancy AI algorithms, and doesn't really even look at what is happening. Despite being pretty stupid, it actually works fairly well.

The core of the algorithm is that each tank compares its position to that of the enemy base. If it is in line with the base horizontally, it faces the base. If it is not in line with the base, it faces in the direction it would need to head to be in line with the base. If the tank can shoot, it shoots. If it can't shoot, it moves forward. I find it easiest to visualise this as an 'L' pattern.
