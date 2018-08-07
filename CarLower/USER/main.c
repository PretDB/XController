#include "stm32f4xx.h" 
#include "timer.h"
#include "move.h"
#include "usart.h"
#include "pwm.h"
#include "delay.h"
#include "led.h"

extern u8 res;
extern u8 res2;
extern u8 res3;
extern int speed;
int mode  = 3;
int sum = 0,dis;
int flag_ultrasonic = 0;

void search_path();
void ultrasonic();
int mea_dis();
void inf();
	
int main(void)
{ 
	LED_Init();
	My_USART1_Init();	 //串口1初始化
	My_USART2_Init();
	My_USART3_Init();
	TIM14_PWM_Init(20000-1,84-1);
	TIM_SetCompare1(TIM14,18400);
	delay_init(168); 
	NVIC_PriorityGroupConfig(NVIC_PriorityGroup_2); //中断优先级管理	 

 	TIM3_Int_Init(4999,0);	//定时器初始化
	TIM4_Int_Init(4999,0);
	
	if(mode == 1)search_path();
	if(mode == 2)ultrasonic();
	if(mode == 3)inf();
}

void search_path()
{
	while(1)
	{ 
		if(res == Stop){
		   Wheel_Stop();
		}
		else if(res == Forward){
		  Car_Forward();
		}
		else if(res == Back){
		  Car_Back();
		}
		else if(res == Left){
		  Car_Left();
		}
		else if(res == Right){
		  Car_Right();
		}else if(res == Left_circle){
		  Car_Left_circle();
		}
		else if(res == Right_circle){
		  Car_Right_circle();
		}
	}
}
void ultrasonic()
{
	int dis_1 = 0,dis_2 = 0,dis_3 = 0,dis_4 = 0,dis_5 = 0,dis_6 = 0;
	int max_index = 0,max = 0;
	
	while(1) //实现比较值从0-300递增，到300后从300-0递减，循环
	{ 
		dis = mea_dis();
		USART_SendData(USART3,dis/256);
		delay_ms(1);
		USART_SendData(USART3,dis%256);
		
		if(dis > 1000){
			Car_Forward();
		}else{
			Wheel_Stop();
			TIM_SetCompare1(TIM14,18125);	
			dis_1 = mea_dis();delay_ms(800);
			TIM_SetCompare1(TIM14,17850);	
			dis_2 = mea_dis();delay_ms(800);
			TIM_SetCompare1(TIM14,17500);	
			dis_3 = mea_dis();delay_ms(800);
			TIM_SetCompare1(TIM14,18675);	
			dis_4 = mea_dis();delay_ms(800);
			TIM_SetCompare1(TIM14,18950);	
			dis_5 = mea_dis();delay_ms(800);
			TIM_SetCompare1(TIM14,19150);	
			dis_6 = mea_dis();delay_ms(800);
			TIM_SetCompare1(TIM14,18400);
			
			if(dis_1 > max){
				max_index = 1;
				max = dis_1;
			}
			if(dis_2 > max){
				max_index = 2;
				max = dis_2;
			}
			if(dis_3 > max){
				max_index = 3;
				max = dis_3;
			}
			if(dis_4 > max){
				max_index = 4;
				max = dis_4;
			}
			if(dis_5 > max){
				max_index = 5;
				max = dis_5;
			}
			if(dis_6 > max){
				max_index = 6;
				max = dis_6;
			}
			
			if((max_index == 1)||(max_index == 2)||(max_index == 3)){GPIO_ResetBits(GPIOF,GPIO_Pin_9);Car_Left_circle();}//
			if((max_index == 4)||(max_index == 5)||(max_index == 6)){GPIO_ResetBits(GPIOF,GPIO_Pin_10);Car_Right_circle();}//
			delay_ms(2000);
			Wheel_Stop();
			GPIO_SetBits(GPIOF,GPIO_Pin_9);
			GPIO_SetBits(GPIOF,GPIO_Pin_10);
			max = 0;
		}

//		TIM_SetCompare1(TIM14,17500);	//修改比较值，修改占空比
//		delay_ms(1000);	 
//		TIM_SetCompare1(TIM14,18400);	//修改比较值，修改占空比
//    delay_ms(1000);	 
//		TIM_SetCompare1(TIM14,19150);	//修改比较值，修改占空比
//		delay_ms(1000);	 
	}
}
int mea_dis(){
	int dis_m;
  while(1){
		USART_SendData(USART2,0x55);
		delay_ms(5);	
    if(flag_ultrasonic == 3){
			flag_ultrasonic = 1;
			dis_m = sum;			
			sum = 0;
			return dis_m;		
		}
	}
}

void inf()
{
	while(1){
	  if((GPIO_ReadInputDataBit(GPIOF, GPIO_Pin_7)==0)||(GPIO_ReadInputDataBit(GPIOF, GPIO_Pin_8)==0)||(GPIO_ReadInputDataBit(GPIOF, GPIO_Pin_5)==0)||(GPIO_ReadInputDataBit(GPIOF, GPIO_Pin_6)==0)){//
			Car_Left_circle();
			GPIO_ResetBits(GPIOF,GPIO_Pin_9);//
		}
		else{
			Car_Forward();
			GPIO_SetBits(GPIOF,GPIO_Pin_9);
		}
	}
}

