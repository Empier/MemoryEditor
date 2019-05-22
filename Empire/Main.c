#pragma warning( disable: 4703)

/*
#include <ntifs.h>           /// IfsKit\Inc
#include <ntddk.h>           /// Inc
#include <wdf.h>             /// Inc\WDF\KMDF\1.9
#include <ndis.h>            /// Inc
#include <fwpmk.h>           /// Inc
#include <fwpsk.h>           /// Inc
#include <netioddk.h>        /// Inc
#include <ntintsafe.h>       /// Inc
#include <ntstrsafe.h>       /// Inc
#include <stdlib.h>          /// SDK\Inc\CRT

*/





#include "Main.h"
//
NTSTATUS UnLoadDriver(PDRIVER_OBJECT DriverObject);
NTSTATUS DisPatchKMS(PDEVICE_OBJECT DeviceObject, PIRP pIrp);


UNICODE_STRING linkNameUnicodeString;
UNICODE_STRING deviceNameUnicodeString;

NTSTATUS DefaultPass(PDEVICE_OBJECT DeviceObject, PIRP pIrp)
{
	pIrp->IoStatus.Status = STATUS_SUCCESS;
	IoCompleteRequest(pIrp, IO_NO_INCREMENT);
	return STATUS_SUCCESS;
}

NTSTATUS MyControl(PDEVICE_OBJECT DeviceObject, PIRP pIrp)
{
	pIrp->IoStatus.Status = STATUS_SUCCESS;

	NTSTATUS ntStatus = STATUS_UNSUCCESSFUL;
	PIO_STACK_LOCATION IrpSp;
	ULONG FunctionCode;
	IrpSp = IoGetCurrentIrpStackLocation(pIrp);
	FunctionCode = IrpSp->Parameters.DeviceIoControl.IoControlCode;
	//DbgPrint("code=%x",FunctionCode);
	switch (FunctionCode)
	{
	case IOCTL_READ_MEMORY:
		
		__try
		{
			struct input
			{
				UINT64 processid;
				UINT64 startaddress;
				WORD bytestoread;
			} *pinp;

			pinp = pIrp->AssociatedIrp.SystemBuffer;
			//ReadProcessMemory();
			
			ntStatus = ReadProcessMemory((DWORD)pinp->processid, NULL, (PVOID)pinp->startaddress, pinp->bytestoread, pinp) ? STATUS_SUCCESS : STATUS_UNSUCCESSFUL;
		}
		__except (1)
		{
			ntStatus = STATUS_UNSUCCESSFUL;
		};
		ntStatus = STATUS_SUCCESS;
		break;
		


	case IOCTL_WRITE_MEMORY:
		__try
		{
			struct input
			{
				UINT64 processid;
				UINT64 startaddress;
				WORD bytestowrite;
			} *pinp, inp;

			pinp = pIrp->AssociatedIrp.SystemBuffer;
			//DbgPrint("%x %x %x", (DWORD)pinp->processid, (PVOID)pinp->startaddress, pinp->bytestowrite);
			ntStatus = WriteProcessMemory((DWORD)pinp->processid, NULL, (PVOID)pinp->startaddress, pinp->bytestowrite, (PVOID)((UINT_PTR)pinp + sizeof(inp))) ? STATUS_SUCCESS : STATUS_UNSUCCESSFUL;
		}
		__except (1)
		{
			//something went wrong and I don't know what
			ntStatus = STATUS_UNSUCCESSFUL;
		};

		ntStatus = STATUS_SUCCESS;
		break;


	case IOCTL_OPEN_PROCESS:
	{
		PEPROCESS selectedprocess;
		ULONG processid = *(PULONG)pIrp->AssociatedIrp.SystemBuffer;
		HANDLE ProcessHandle;

		ntStatus = STATUS_SUCCESS;

		__try
		{
			ProcessHandle = 0;

			if (PsLookupProcessByProcessId((PVOID)(UINT_PTR)(processid), &selectedprocess) == STATUS_SUCCESS)
			{
				ntStatus = ObOpenObjectByPointer(
					selectedprocess,
					0,
					NULL,
					PROCESS_ALL_ACCESS,
					*PsProcessType,
					KernelMode, //UserMode,
					&ProcessHandle);

				//DbgPrint("ntStatus=%x", ntStatus);
			}
		}
		__except (1)
		{
			ntStatus = STATUS_UNSUCCESSFUL;
		}

		*(PUINT64)pIrp->AssociatedIrp.SystemBuffer = (UINT64)ProcessHandle;
		break;
	}
	default:

		ntStatus = STATUS_SUCCESS;
		break;
	}










	pIrp->IoStatus.Status = ntStatus;

	if (IrpSp) //only NULL when loaded by dbvm
	{
		if (ntStatus == STATUS_SUCCESS)
			pIrp->IoStatus.Information = IrpSp->Parameters.DeviceIoControl.OutputBufferLength;
		else
			pIrp->IoStatus.Information = 0;

		IoCompleteRequest(pIrp, IO_NO_INCREMENT);
	}
	return ntStatus;
}


NTSTATUS DriverEntry(PDRIVER_OBJECT DriverObject, PUNICODE_STRING RegisterPath)
{
	NTSTATUS Status = STATUS_UNSUCCESSFUL;
	
	PDEVICE_OBJECT pMyDevice;
	
	DbgPrint("___[Empire Kernel Load]___");
	
	
	RtlInitUnicodeString(&deviceNameUnicodeString, L"\\Device\\Empire2");
	Status = IoCreateDevice(DriverObject, 4, &deviceNameUnicodeString, FILE_DEVICE_UNKNOWN, 0, TRUE, &pMyDevice);
	if (!NT_SUCCESS(Status))
	{
		DbgPrint("Failed to create the device!\n");
		return Status;
	}

	RtlInitUnicodeString(&linkNameUnicodeString, L"\\DosDevices\\Empire2");
	Status = IoCreateSymbolicLink(&linkNameUnicodeString, &deviceNameUnicodeString);
	if (!NT_SUCCESS(Status))
	{
		DbgPrint("Failed to create the symlink\n");
		return Status;
	}

	for (int nIndex = 0; nIndex < IRP_MJ_MAXIMUM_FUNCTION; nIndex++)
	{
		DriverObject->MajorFunction[nIndex] = DefaultPass;

	}

	DriverObject->MajorFunction[IRP_MJ_DEVICE_CONTROL] = MyControl;

	

	
	DriverObject->DriverUnload = UnLoadDriver;
	Status = STATUS_SUCCESS;

	return Status;
}


NTSTATUS UnLoadDriver(PDRIVER_OBJECT DriverObject)
{

	NTSTATUS Status = STATUS_SUCCESS;
	DbgPrint("___[Empire Kernel Unload Success!]___");
	IoDeleteSymbolicLink(&linkNameUnicodeString);
	IoDeleteDevice(DriverObject->DeviceObject);
	
Exit0:
	return Status;
}

