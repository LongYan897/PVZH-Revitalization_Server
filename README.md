# PVZH-Revitalization_Server
服务端    

api端口：     
/api/Game/register    
请求体：    
{    
	DisplayName : <填写注册用名字>    
	Password : <注册密码>    
}    
返回体：    
{    
	Code : 处理结果码(0 = 成功，3 = 名字存在, 4 = 名字或密码不能是空格    
	DisplayName : 确认名字    
	Token : 请求GameHub及查询服务的令牌，1个月后过期     
}    
该api访问不需要token   

/api/Game/login   
请求体：   
{    
	DisplayName : <填写登录用名字>    
	Password : <登录密码>     
}     
返回体:    
{    
	Code : 处理结果码(0 = 成功，1 = 玩家不存在, 2 = 密码错误      
	DisplayName : 确认名字     
	Token : 请求GameHub及查询服务的令牌，1个月后过期     
}    
该api访问不需要token    

/api/Game/get/profile     
无需请求体    
返回体：     
{     
	DisplayName : 玩家名
	Code : 处理结果码，结果只可能为0    
}     
该api访问需要token    

GameHub 请求函数：##函数均为单参数，需要请求格式    

SendFriendRequest    
请求格式:    
{    
	FriendName = 请求对象的名称    
}    
返回体：    
{    
	Code = 处理结果码(0=成功,1 = 玩家不存在,2 = 不能重复对一个玩家请求)    
}   

SendFriendRequest   
无需请求体    
返回体：   
{    
	Requests = [{    
		SenderName = 请求者的名字    
	},...]    
}     

ApproveFriendReques    
请求格式：    
{    
	FriendName = 要回复好友请求的玩家名称,     
	Accept = 是否同意（bool值）    
}    
返回体：   
{   
	Code = 处理结果码(0 = 成功, 1 = 未找到请求者)   
	RequestResult = 请求结果码(0 = 接受，1 = 拒绝)，仅用于核对   
}   

GetRelation
无需请求体  
返回体：  
{  
	Relations = [{  
		Player1Name = 好友关系其中一名玩家名,  
		Player2Name = 好友关系其中一名玩家名   
	},...]  
}  