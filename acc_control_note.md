# controling with accelerometer
basically, controlling car with accelerometer is implemented by judging the symbol of x and y axis.

1. Forward if abs(x) < 1 and y < 2
2. Backward if abs(x) < 1 and y > 6
3. LeftRotate if abs(x) > 3 and y > 4
4. RigthRotate if abs(x) < -3 and y > 4
5. LeftShift if abs(y) < 2 and x > 3
6. RightShift if abs(y) < 2 and x < -3
