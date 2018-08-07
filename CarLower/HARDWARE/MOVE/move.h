#ifndef __MOVE_H
#define __MOVE_H	 

#define Stop 0//小车运动方向
#define Forward 1
#define Back 2
#define Left 3
#define Right 4
#define Left_circle 5
#define Right_circle 6
//#define Left_forward 113
//#define Right_forward 101
//#define Left_back 122
//#define Right_back 99
//#define Low 3
//#define High 4

void Wheel_Stop(void);//轮子停止
void Wheel_Forward_1(void);//轮子1前进
void Wheel_Forward_2(void);//轮子2前进
void Wheel_Forward_3(void);//轮子3前进
void Wheel_Forward_4(void);//轮子4前进

void Wheel_Back_1(void);//轮子1后退
void Wheel_Back_2(void);//轮子2后退
void Wheel_Back_3(void);//轮子3后退
void Wheel_Back_4(void);//轮子4后退
		 				    
void Car_Forward(void);//小车前进
void Car_Back(void);//小车后退
void Car_Left(void);//小车左拐
void Car_Right(void);//小车右拐
void Car_Left_Forward(void);//小车左上方移动
void Car_Right_forward(void);//小车右上方移动
void Car_Left_Back(void);//小车左后方移动
void Car_Right_Back(void);//小车右后方移动

void Car_Left_circle(void);//小车左打圈
void Car_Right_circle(void);//小车右打圈
		
#endif

