#include "move.h"
#include "stm32f4xx.h" 

int speed = 1500;
/*********************************ͣ��**************************************/
void Wheel_Stop(void)
{
    TIM_SetCompare1(TIM3,0);		
    TIM_SetCompare2(TIM3,0);   
    TIM_SetCompare3(TIM3,0);		
		TIM_SetCompare4(TIM3,0);	  
		
		TIM_SetCompare1(TIM4,0);		
    TIM_SetCompare2(TIM4,0);   
    TIM_SetCompare3(TIM4,0);		
		TIM_SetCompare4(TIM4,0);	  
}
/****************************************************************************/

/*********************************������ǰ*******************************************/
void Wheel_Forward_1(void)
{
	TIM_SetCompare1(TIM3,0);   //1��
  TIM_SetCompare2(TIM3,speed);   //1��
}

void Wheel_Forward_2(void)
{
	TIM_SetCompare3(TIM3,0);	  //2��
  TIM_SetCompare4(TIM3,speed);	  //2��
}

void Wheel_Forward_3(void)
{
  TIM_SetCompare1(TIM4,speed);		//3��		
	TIM_SetCompare2(TIM4,0);		//3��	
}

void Wheel_Forward_4(void)
{
	TIM_SetCompare3(TIM4,0);	  //4��
  TIM_SetCompare4(TIM4,speed);	  //4��
}
/**********************************************************************************/

/**********************************�����Ⱥ�*********************************************/
void Wheel_Back_1(void)
{
  TIM_SetCompare1(TIM3,speed);		//1��	
	TIM_SetCompare2(TIM3,0);		//1��	
}

void Wheel_Back_2(void)
{
  TIM_SetCompare3(TIM3,speed);		//2��		
	TIM_SetCompare4(TIM3,0);		//2��	
}


void Wheel_Back_3(void)
{
	TIM_SetCompare1(TIM4,0);   //3��
  TIM_SetCompare2(TIM4,speed);   //3��
}


void Wheel_Back_4(void)
{
  TIM_SetCompare3(TIM4,speed);		//4��
	TIM_SetCompare4(TIM4,0);		//4��
}
/*********************************************************************************/

/*********************************С�����ƶ�**************************************************/
void Car_Forward(void)
{
  TIM_SetCompare2(TIM3,speed);   //1��
	TIM_SetCompare1(TIM3,0);   //1��
	TIM_SetCompare4(TIM3,speed);	  //2��
	TIM_SetCompare3(TIM3,0);	  //2��
	TIM_SetCompare1(TIM4,speed);		//3��
	TIM_SetCompare2(TIM4,0);		//3��	
	TIM_SetCompare4(TIM4,speed);	  //4��
	TIM_SetCompare3(TIM4,0);	  //4��
}

void Car_Back(void)
{
  TIM_SetCompare1(TIM3,speed);		//1��
	TIM_SetCompare2(TIM3,0);		//1��	
	TIM_SetCompare3(TIM3,speed);		//2��
	TIM_SetCompare4(TIM3,0);		//2��	
	TIM_SetCompare2(TIM4,speed);   //3��
	TIM_SetCompare1(TIM4,0);   //3��
	TIM_SetCompare3(TIM4,speed);		//4��
	TIM_SetCompare4(TIM4,0);		//4��
}

void Car_Left(void)
{
   TIM_SetCompare1(TIM3,speed);		//1��
	TIM_SetCompare2(TIM3,0);		//1��	
   TIM_SetCompare4(TIM3,speed);	  //2��
	TIM_SetCompare3(TIM3,0);	  //2��
	 TIM_SetCompare2(TIM4,speed);   //3��
	TIM_SetCompare1(TIM4,0);   //3��
	 TIM_SetCompare4(TIM4,speed);	  //4��
	TIM_SetCompare3(TIM4,0);	  //4��
}

void Car_Right(void)
{
   TIM_SetCompare2(TIM3,speed);   //1��
	TIM_SetCompare1(TIM3,0);   //1��
   TIM_SetCompare3(TIM3,speed);		//2��
	TIM_SetCompare4(TIM3,0);		//2��	
   TIM_SetCompare1(TIM4,speed);		//3��
	TIM_SetCompare2(TIM4,0);		//3��	
	 TIM_SetCompare3(TIM4,speed);		//4��
	TIM_SetCompare4(TIM4,0);		//4��
}

void Car_Left_Forward(void)
{
	TIM_SetCompare1(TIM3,0);	  //1ͣ
	TIM_SetCompare2(TIM3,0);	  //1ͣ
  TIM_SetCompare4(TIM3,speed);	  //2��
	TIM_SetCompare3(TIM3,0);	  //2��
	TIM_SetCompare1(TIM4,0);	  //3ͣ
	TIM_SetCompare2(TIM4,0);	  //3ͣ
	TIM_SetCompare4(TIM4,speed);	  //4��
	TIM_SetCompare3(TIM4,0);	  //4��
	
}

void Car_Right_forward(void)
{
  TIM_SetCompare2(TIM3,speed);   //1��
	TIM_SetCompare1(TIM3,0);   //1��
	TIM_SetCompare2(TIM3,0);   //2ͣ
	TIM_SetCompare1(TIM3,0);   //2ͣ
  TIM_SetCompare1(TIM4,speed);		//3��
	TIM_SetCompare2(TIM4,0);		//3��	
	TIM_SetCompare3(TIM4,0);		//4ͣ
	TIM_SetCompare4(TIM4,0);		//4ͣ
}

void Car_Left_Back(void)
{
  TIM_SetCompare1(TIM3,speed);		//1��
	TIM_SetCompare2(TIM3,0);		//1��	
	TIM_SetCompare2(TIM3,speed);   //2ͣ
	TIM_SetCompare1(TIM3,0);   //2ͣ
  TIM_SetCompare2(TIM4,speed);   //3��
	TIM_SetCompare1(TIM4,0);   //3��
	TIM_SetCompare3(TIM4,0);		//4ͣ
	TIM_SetCompare4(TIM4,0);		//4ͣ
}

void Car_Right_Back(void)
{
	TIM_SetCompare1(TIM3,0);	  //1ͣ
	TIM_SetCompare2(TIM3,0);	  //1ͣ
  TIM_SetCompare3(TIM3,speed);		//2��
	TIM_SetCompare4(TIM3,0);		//2��	
	TIM_SetCompare1(TIM4,0);	  //3ͣ
	TIM_SetCompare2(TIM4,0);	  //3ͣ
	TIM_SetCompare3(TIM4,speed);		//4��
	TIM_SetCompare4(TIM4,0);		//4��
}

void Car_Left_circle(void)
{
  TIM_SetCompare1(TIM3,speed);		//1��
	TIM_SetCompare2(TIM3,0);		//1��	
  TIM_SetCompare4(TIM3,speed);	  //2��
	TIM_SetCompare3(TIM3,0);	  //2��
  TIM_SetCompare1(TIM4,speed);		//3��
	TIM_SetCompare2(TIM4,0);		//3��	
	TIM_SetCompare3(TIM4,speed);		//4��
	TIM_SetCompare4(TIM4,0);		//4��
}

void Car_Right_circle(void)
{
  TIM_SetCompare1(TIM3,0);		//1��
	TIM_SetCompare2(TIM3,speed);		//1��
  TIM_SetCompare4(TIM3,0);	  //2��
	TIM_SetCompare3(TIM3,speed);	  //2��
  TIM_SetCompare1(TIM4,0);		//3��
	TIM_SetCompare2(TIM4,speed);		//3��
	TIM_SetCompare3(TIM4,0);		//4��
	TIM_SetCompare4(TIM4,speed);		//4��
}

/************************************************************************/


















