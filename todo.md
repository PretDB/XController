# TODO

Add multi-client control. Currently, only one client can control the specific car. The reason may be that the connection of last client did not close, so that the other client can not connect to the car. Will it works if closing the tcp connection after a valid command sent? If does, the procedure of sending command would looks like below:
1. connect the specific car via tcp connection.
2. send command, get status.
3. close connection.
The upon procedure has been done in MessageEmitter, too.

[ ] add speed and fire in args in controll pack



