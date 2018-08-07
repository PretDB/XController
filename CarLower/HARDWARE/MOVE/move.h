#ifndef __MOVE_H
#define __MOVE_H	 

#define Stop 0//С���˶�����
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

void Wheel_Stop(void);//����ֹͣ
void Wheel_Forward_1(void);//����1ǰ��
void Wheel_Forward_2(void);//����2ǰ��
void Wheel_Forward_3(void);//����3ǰ��
void Wheel_Forward_4(void);//����4ǰ��

void Wheel_Back_1(void);//����1����
void Wheel_Back_2(void);//����2����
void Wheel_Back_3(void);//����3����
void Wheel_Back_4(void);//����4����
		 				    
void Car_Forward(void);//С��ǰ��
void Car_Back(void);//С������
void Car_Left(void);//С�����
void Car_Right(void);//С���ҹ�
void Car_Left_Forward(void);//С�����Ϸ��ƶ�
void Car_Right_forward(void);//С�����Ϸ��ƶ�
void Car_Left_Back(void);//С������ƶ�
void Car_Right_Back(void);//С���Һ��ƶ�

void Car_Left_circle(void);//С�����Ȧ
void Car_Right_circle(void);//С���Ҵ�Ȧ
		
#endif

