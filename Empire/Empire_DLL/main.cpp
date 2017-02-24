#include <Windows.h>
#include <stdio.h>
HANDLE kernel = 0;
/*

BYTE ao[512] = { 0x20, };
DWORD k = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x2000, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
DWORD wr = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x2001, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
DWORD k3 = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x2002, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
result = CreateFileW(L"\\\\.\\Empire", 0xC0000000, 3u, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);
//if ((int)result != -1){
printf("Val=%d\n", result);


i.processid = 0x1bc8;
i.startaddress = 0x5bc000;
i.bytestoread = 4;

j.processid = 0x1bc8;
j.startaddress = 0x5bc000;
j.bytestoread = 8;
printf("%d\n", sizeof(input));




//memset(&j, 4, 512);

memcpy(ao, &j, sizeof(input));
memset(ao + sizeof(input), 0x90, 8);


//DeviceIoControl(result,k, (LPVOID)&i, sizeof(i), (LPVOID)&OutBuffer, 4,  &BytesReturned, 0);

//printf("memory: %d\n", OutBuffer);

printf("%d\n", DeviceIoControl(result, wr, (LPVOID)&ao, 512, (LPVOID)&ao, 512, &BytesReturned, 0));

















#include <Windows.h>
#include <stdio.h>



int main()
{
if (LoadDriver()){
printf("Loaded the driver successfully!\n");
printf("Press ENTER to unload the driver: ");
getchar();
if (UnloadDriver()) printf("Unloaded the driver successfully!\n");
else printf("Failed to unload the driver!\n");
}
else printf("Failed to load the driver!\n");


return 0;
}



*/

//extern __declspec(dllimport) bool test();

BOOL LoadDriver(void);
BOOL UnloadDriver(void);

const wchar_t MY_DRIVER[] = L"EMPIRE";
wchar_t MY_PATH[MAX_PATH] = L"";
wchar_t MY_DRIVER_NAME[MAX_PATH] = L"\\Empire.sys";
BOOL LoadDriver(){
	SC_HANDLE hSCM;
	SC_HANDLE hService;

	hSCM = OpenSCManager(NULL, NULL, SC_MANAGER_ALL_ACCESS);
	if (!hSCM){
		printf("Unable to open the service control manager...\n");
		return FALSE;
	}


	GetCurrentDirectory(MAX_PATH, MY_PATH);

	wcscat(MY_PATH, MY_DRIVER_NAME);

	hService = CreateService(hSCM, MY_DRIVER, MY_DRIVER, SERVICE_ALL_ACCESS, SERVICE_KERNEL_DRIVER, SERVICE_DEMAND_START, SERVICE_ERROR_NORMAL,
		MY_PATH, NULL, NULL, NULL, NULL, NULL);
	if (!hService){
		hService = OpenService(hSCM, MY_DRIVER, SERVICE_ALL_ACCESS);
		if (!hService)
		{
			printf("Unable to create/open the service....\n");
			CloseServiceHandle(hSCM);
			return FALSE;
		}
	}

	if (!StartService(hService, 0, NULL) &&
		(GetLastError() != ERROR_SERVICE_ALREADY_RUNNING)){
		printf("Unable to start the service....%d\n", GetLastError());
		CloseServiceHandle(hSCM);
		CloseServiceHandle(hService);
		return FALSE;
	}

	CloseServiceHandle(hSCM);
	CloseServiceHandle(hService);

	return TRUE;
}

// 드라이버를 언로드하는 함수
BOOL UnloadDriver(){
	SC_HANDLE hSCM;
	SC_HANDLE hService;
	SERVICE_STATUS svcStatus;

	hSCM = OpenSCManager(NULL, NULL, SC_MANAGER_ALL_ACCESS);
	if (!hSCM){
		printf("Unable to open the service control manager...\n");
		return FALSE;
	}

	hService = OpenService(hSCM, MY_DRIVER, SERVICE_ALL_ACCESS);
	if (!hService){
		printf("Unable to open the service...\n");
		return FALSE;
	}


	if (!ControlService(hService, SERVICE_CONTROL_STOP, &svcStatus))
	{
		printf("Unable to stop the service..\n");
		CloseServiceHandle(hSCM);
		CloseServiceHandle(hService);
		return FALSE;
	}


	if (!DeleteService(hService))
	{
		printf("Unable to delete the service..\n");
		CloseServiceHandle(hSCM);
		CloseServiceHandle(hService);
		return FALSE;
	}

	CloseServiceHandle(hSCM);
	CloseServiceHandle(hService);
}


extern "C" __declspec(dllexport) void test()
{
	MessageBoxA(NULL, "A", "B", MB_OK);
	
}
extern "C" __declspec(dllexport) BOOL WPM(UINT64 pid, UINT64 startaddress, WORD bytestowrite,DWORD *w)
{
	DWORD BytesReturned;

	BYTE data[512] = { 0x20, };


	struct input
	{
		UINT64 processid;
		UINT64 startaddress;
		WORD bytestoread;
	} i;

	i.processid = pid;
	i.startaddress = startaddress;
	i.bytestoread = bytestowrite;

	memcpy(data, &i, sizeof(input));
	//memset(data + sizeof(input), 0x0, bytestowrite);
	memcpy(data + sizeof(input), w, bytestowrite);

	DWORD cc = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x2001, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);


	DeviceIoControl(kernel, cc, (LPVOID)&data, 512, (LPVOID)&data, 512, &BytesReturned, 0);

	return 0;
}


extern "C"  __declspec(dllexport) BOOL RPM(UINT64 pid, UINT64 startaddress, WORD bytestoread, DWORD *r)
{
	DWORD BytesReturned;
	DWORD OutBuffer[512];
	struct input
	{
		UINT64 processid;
		UINT64 startaddress;
		WORD bytestoread;
	} i;

	i.processid = pid;
	i.startaddress = startaddress;
	i.bytestoread = bytestoread;

	DWORD cc = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x2000, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
	

	DeviceIoControl(kernel,cc, (LPVOID)&i, sizeof(i), (LPVOID)r, bytestoread,  &BytesReturned, 0);



	//printf("memory: %d\n", OutBuffer);

	


	return 0;
	
}

extern "C" __declspec(dllexport) BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
	switch (fdwReason)
	{
	case DLL_PROCESS_ATTACH:
		LoadDriver();
		kernel = CreateFileW(L"\\\\.\\Empire", 0xC0000000, 3u, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);
		
		//if (LoadDriver()){
		//	printf("Loaded the driver successfully!\n");
		//	printf("Press ENTER to unload the driver: ");
		//}
		//else printf("Failed to load the driver!\n");
		break;

	case DLL_PROCESS_DETACH:
		//kernel = CreateFileW(L"\\\\.\\Empire", 0xC0000000, 3u, 0, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);


		CloseHandle(kernel);

		//if (UnloadDriver()) printf("Unloaded the driver successfully!\n");
		//else printf("Failed to unload the driver!\n");
		UnloadDriver();
		break;

	case DLL_THREAD_ATTACH:
		break;

	case DLL_THREAD_DETACH:
		break;
	}
	return TRUE; // succesful
}
