#include "move.h"
#include "stm32f4xx.h" 

int speed = 1500;
/*********************************停车**************************************/
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

/*********************************轮子先前*******************************************/
void Wheel_Forward_1(void)
{
	TIM_SetCompare1(TIM3,0);   //1正
  TIM_SetCompare2(TIM3,speed);   //1正
}

void Wheel_Forward_2(void)
{
	TIM_SetCompare3(TIM3,0);	  //2正
  TIM_SetCompare4(TIM3,speed);	  //2正
}

void Wheel_Forward_3(void)
{
  TIM_SetCompare1(TIM4,speed);		//3正		
	TIM_SetCompare2(TIM4,0);		//3正	
}

void Wheel_Forward_4(void)
{
	TIM_SetCompare3(TIM4,0);	  //4正
  TIM_SetCompare4(TIM4,speed);	  //4正
}
/**********************************************************************************/

/**********************************轮子先后*********************************************/
void Wheel_Back_1(void)
{
  TIM_SetCompare1(TIM3,speed);		//1倒	
	TIM_SetCompare2(TIM3,0);		//1倒	
}

void Wheel_Back_2(void)
{
  TIM_SetCompare3(TIM3,speed);		//2倒		
	TIM_SetCompare4(TIM3,0);		//2倒	
}


void Wheel_Back_3(void)
{
	TIM_SetCompare1(TIM4,0);   //3倒
  TIM_SetCompare2(TIM4,speed);   //3倒
}


void Wheel_Back_4(void)
{
  TIM_SetCompare3(TIM4,speed);		//4倒
	TIM_SetCompare4(TIM4,0);		//4倒
}
/*********************************************************************************/

/*********************************小车的移动**************************************************/
void Car_Forward(void)
{
  TIM_SetCompare2(TIM3,speed);   //1正
	TIM_SetCompare1(TIM3,0);   //1正
	TIM_SetCompare4(TIM3,speed);	  //2正
	TIM_SetCompare3(TIM3,0);	  //2正
	TIM_SetCompare1(TIM4,speed);		//3正
	TIM_SetCompare2(TIM4,0);		//3正	
	TIM_SetCompare4(TIM4,speed);	  //4正
	TIM_SetCompare3(TIM4,0);	  //4正
}

void Car_Back(void)
{
  TIM_SetCompare1(TIM3,speed);		//1倒
	TIM_SetCompare2(TIM3,0);		//1倒	
	TIM_SetCompare3(TIM3,speed);		//2倒
	TIM_SetCompare4(TIM3,0);		//2倒	
	TIM_SetCompare2(TIM4,speed);   //3倒
	TIM_SetCompare1(TIM4,0);   //3倒
	TIM_SetCompare3(TIM4,speed);		//4倒
	TIM_SetCompare4(TIM4,0);		//4倒
}

void Car_Left(void)
{
   TIM_SetCompare1(TIM3,speed);		//1倒
	TIM_SetCompare2(TIM3,0);		//1倒	
   TIM_SetCompare4(TIM3,speed);	  //2正
	TIM_SetCompare3(TIM3,0);	  //2正
	 TIM_SetCompare2(TIM4,speed);   //3倒
	TIM_SetCompare1(TIM4,0);   //3倒
	 TIM_SetCompare4(TIM4,speed);	  //4正
	TIM_SetCompare3(TIM4,0);	  //4正
}

void Car_Right(void)
{
   TIM_SetCompare2(TIM3,speed);   //1正
	TIM_SetCompare1(TIM3,0);   //1正
   TIM_SetCompare3(TIM3,speed);		//2倒
	TIM_SetCompare4(TIM3,0);		//2倒	
   TIM_SetCompare1(TIM4,speed);		//3正
	TIM_SetCompare2(TIM4,0);		//3正	
	 TIM_SetCompare3(TIM4,speed);		//4倒
	TIM_SetCompare4(TIM4,0);		//4倒
}

void Car_Left_Forward(void)
{
	TIM_SetCompare1(TIM3,0);	  //1停
	TIM_SetCompare2(TIM3,0);	  //1停
  TIM_SetCompare4(TIM3,speed);	  //2正
	TIM_SetCompare3(TIM3,0);	  //2正
	TIM_SetCompare1(TIM4,0);	  //3停
	TIM_SetCompare2(TIM4,0);	  //3停
	TIM_SetCompare4(TIM4,speed);	  //4正
	TIM_SetCompare3(TIM4,0);	  //4正
	
}

void Car_Right_forward(void)
{
  TIM_SetCompare2(TIM3,speed);   //1正
	TIM_SetCompare1(TIM3,0);   //1正
	TIM_SetCompare2(TIM3,0);   //2停
	TIM_SetCompare1(TIM3,0);   //2停
  TIM_SetCompare1(TIM4,speed);		//3正
	TIM_SetCompare2(TIM4,0);		//3正	
	TIM_SetCompare3(TIM4,0);		//4停
	TIM_SetCompare4(TIM4,0);		//4停
}

void Car_Left_Back(void)
{
  TIM_SetCompare1(TIM3,speed);		//1倒
	TIM_SetCompare2(TIM3,0);		//1倒	
	TIM_SetCompare2(TIM3,speed);   //2停
	TIM_SetCompare1(TIM3,0);   //2停
  TIM_SetCompare2(TIM4,speed);   //3倒
	TIM_SetCompare1(TIM4,0);   //3倒
	TIM_SetCompare3(TIM4,0);		//4停
	TIM_SetCompare4(TIM4,0);		//4停
}

void Car_Right_Back(void)
{
	TIM_SetCompare1(TIM3,0);	  //1停
	TIM_SetCompare2(TIM3,0);	  //1停
  TIM_SetCompare3(TIM3,speed);		//2倒
	TIM_SetCompare4(TIM3,0);		//2倒	
	TIM_SetCompare1(TIM4,0);	  //3停
	TIM_SetCompare2(TIM4,0);	  //3停
	TIM_SetCompare3(TIM4,speed);		//4倒
	TIM_SetCompare4(TIM4,0);		//4倒
}

void Car_Left_circle(void)
{
  TIM_SetCompare1(TIM3,speed);		//1倒
	TIM_SetCompare2(TIM3,0);		//1倒	
  TIM_SetCompare4(TIM3,speed);	  //2正
	TIM_SetCompare3(TIM3,0);	  //2正
  TIM_SetCompare1(TIM4,speed);		//3正
	TIM_SetCompare2(TIM4,0);		//3正	
	TIM_SetCompare3(TIM4,speed);		//4倒
	TIM_SetCompare4(TIM4,0);		//4倒
}

void Car_Right_circle(void)
{
  TIM_SetCompare1(TIM3,0);		//1正
	TIM_SetCompare2(TIM3,speed);		//1正
  TIM_SetCompare4(TIM3,0);	  //2倒
	TIM_SetCompare3(TIM3,speed);	  //2倒
  TIM_SetCompare1(TIM4,0);		//3倒
	TIM_SetCompare2(TIM4,speed);		//3倒
	TIM_SetCompare3(TIM4,0);		//4正
	TIM_SetCompare4(TIM4,speed);		//4正
}

/************************************************************************/


















