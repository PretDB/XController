#include "timer.h"
#include "led.h"
void TIM3_Int_Init(u16 arr,u16 psc)
{
	TIM_TimeBaseInitTypeDef TIM_TimeBaseInitStructure;
	GPIO_InitTypeDef GPIO_InitStructure;
	TIM_OCInitTypeDef  TIM_OCInitStructure;
	
	RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM3,ENABLE);  
	RCC_AHB1PeriphClockCmd(RCC_AHB1Periph_GPIOC, ENABLE); 
	
	GPIO_PinAFConfig(GPIOC,GPIO_PinSource6,GPIO_AF_TIM3); 
	GPIO_PinAFConfig(GPIOC,GPIO_PinSource7,GPIO_AF_TIM3);
	GPIO_PinAFConfig(GPIOC,GPIO_PinSource8,GPIO_AF_TIM3); 
	GPIO_PinAFConfig(GPIOC,GPIO_PinSource9,GPIO_AF_TIM3);
	
	GPIO_InitStructure.GPIO_Pin = GPIO_Pin_6;          
	GPIO_InitStructure.GPIO_Mode = GPIO_Mode_AF;       
	GPIO_InitStructure.GPIO_Speed = GPIO_Speed_50MHz;	
	GPIO_Init(GPIOC,&GPIO_InitStructure);   

  GPIO_InitStructure.GPIO_Pin = GPIO_Pin_7; 
  GPIO_Init(GPIOC,&GPIO_InitStructure); 
  GPIO_InitStructure.GPIO_Pin = GPIO_Pin_8; 
  GPIO_Init(GPIOC,&GPIO_InitStructure);  	
  GPIO_InitStructure.GPIO_Pin = GPIO_Pin_9; 
  GPIO_Init(GPIOC,&GPIO_InitStructure);  	

  TIM_TimeBaseInitStructure.TIM_Period = arr; 	
	TIM_TimeBaseInitStructure.TIM_Prescaler=psc; 
	TIM_TimeBaseInitStructure.TIM_CounterMode=TIM_CounterMode_Up; 
	TIM_TimeBaseInitStructure.TIM_ClockDivision=TIM_CKD_DIV1; 
	
	TIM_TimeBaseInit(TIM3,&TIM_TimeBaseInitStructure);
	
	TIM_OCInitStructure.TIM_OCMode = TIM_OCMode_PWM1; 
 	TIM_OCInitStructure.TIM_OutputState = TIM_OutputState_Enable;
	TIM_OCInitStructure.TIM_OCPolarity = TIM_OCPolarity_High; 
																															
	TIM_OC1Init(TIM3, &TIM_OCInitStructure);  
	TIM_OC2Init(TIM3, &TIM_OCInitStructure);  
	TIM_OC3Init(TIM3, &TIM_OCInitStructure);  
	TIM_OC4Init(TIM3, &TIM_OCInitStructure);  
	
	TIM_OC1PreloadConfig(TIM3, TIM_OCPreload_Enable); 
	TIM_OC2PreloadConfig(TIM3, TIM_OCPreload_Enable); 
	TIM_OC3PreloadConfig(TIM3, TIM_OCPreload_Enable); 
	TIM_OC4PreloadConfig(TIM3, TIM_OCPreload_Enable); 
 
  TIM_ARRPreloadConfig(TIM3,ENABLE);

	TIM_ITConfig(TIM3,TIM_IT_Update,ENABLE); 
	TIM_Cmd(TIM3,ENABLE); 
}


void TIM4_Int_Init(u16 arr,u16 psc)
{
	TIM_TimeBaseInitTypeDef TIM_TimeBaseInitStructure;
	GPIO_InitTypeDef GPIO_InitStructure;
	TIM_OCInitTypeDef  TIM_OCInitStructure;
	
	RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM4,ENABLE);  
	RCC_AHB1PeriphClockCmd(RCC_AHB1Periph_GPIOB, ENABLE); 
	
	GPIO_PinAFConfig(GPIOB,GPIO_PinSource6,GPIO_AF_TIM4); 
	GPIO_PinAFConfig(GPIOB,GPIO_PinSource7,GPIO_AF_TIM4);
	GPIO_PinAFConfig(GPIOB,GPIO_PinSource8,GPIO_AF_TIM4); 
	GPIO_PinAFConfig(GPIOB,GPIO_PinSource9,GPIO_AF_TIM4);
	
	GPIO_InitStructure.GPIO_Pin = GPIO_Pin_6;          
	GPIO_InitStructure.GPIO_Mode = GPIO_Mode_AF;       
	GPIO_InitStructure.GPIO_Speed = GPIO_Speed_50MHz;	
	GPIO_Init(GPIOB,&GPIO_InitStructure);   

  GPIO_InitStructure.GPIO_Pin = GPIO_Pin_7; 
  GPIO_Init(GPIOB,&GPIO_InitStructure); 
  GPIO_InitStructure.GPIO_Pin = GPIO_Pin_8; 
  GPIO_Init(GPIOB,&GPIO_InitStructure);  	
  GPIO_InitStructure.GPIO_Pin = GPIO_Pin_9; 
  GPIO_Init(GPIOB,&GPIO_InitStructure);  	

  TIM_TimeBaseInitStructure.TIM_Period = arr; 	
	TIM_TimeBaseInitStructure.TIM_Prescaler=psc; 
	TIM_TimeBaseInitStructure.TIM_CounterMode=TIM_CounterMode_Up; 
	TIM_TimeBaseInitStructure.TIM_ClockDivision=TIM_CKD_DIV1; 
	
	TIM_TimeBaseInit(TIM4,&TIM_TimeBaseInitStructure);
	
	TIM_OCInitStructure.TIM_OCMode = TIM_OCMode_PWM1; 
 	TIM_OCInitStructure.TIM_OutputState = TIM_OutputState_Enable;
	TIM_OCInitStructure.TIM_OCPolarity = TIM_OCPolarity_High; 
																															
	TIM_OC1Init(TIM4, &TIM_OCInitStructure);  
	TIM_OC2Init(TIM4, &TIM_OCInitStructure);  
	TIM_OC3Init(TIM4, &TIM_OCInitStructure);  
	TIM_OC4Init(TIM4, &TIM_OCInitStructure);  
	
	TIM_OC1PreloadConfig(TIM4, TIM_OCPreload_Enable); 
	TIM_OC2PreloadConfig(TIM4, TIM_OCPreload_Enable); 
	TIM_OC3PreloadConfig(TIM4, TIM_OCPreload_Enable); 
	TIM_OC4PreloadConfig(TIM4, TIM_OCPreload_Enable); 
 
  TIM_ARRPreloadConfig(TIM4,ENABLE);

	TIM_ITConfig(TIM4,TIM_IT_Update,ENABLE); 
	TIM_Cmd(TIM4,ENABLE);
}

