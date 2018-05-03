#include "Main.h"
#include "memory_module.h"


BOOLEAN WriteProcessMemory(DWORD PID, PEPROCESS PEProcess, PVOID Address, DWORD Size, PVOID Buffer)
{
	PEPROCESS selectedprocess = PEProcess;
	KAPC_STATE apc_state;
	NTSTATUS ntStatus = STATUS_UNSUCCESSFUL;

	if (selectedprocess == NULL)
	{
		//DbgPrint("WriteProcessMemory:Getting PEPROCESS\n");
		if (!NT_SUCCESS(PsLookupProcessByProcessId((PVOID)(UINT_PTR)PID, &selectedprocess)))
			return FALSE; //couldn't get the PID

		//DbgPrint("Retrieved peprocess");  
	}

	//selectedprocess now holds a valid peprocess value
	__try
	{
		UINT_PTR temp = (UINT_PTR)Address;

		RtlZeroMemory(&apc_state, sizeof(apc_state));

		KeAttachProcess((PEPROCESS)selectedprocess);

		__try
		{
			char* target;
			char* source;
			unsigned int i;

			//DbgPrint("Checking safety of memory\n");

			if ((IsAddressSafe((UINT_PTR)Address)) && (IsAddressSafe((UINT_PTR)Address + Size - 1)))
			{

				target = (char *)Address;
				source = (char *)Buffer;


				__writecr0(__readcr0() & ~0x10000);
				//DbgPrint("%x %x", target[0], source[0]);
				//	Size = 1;
				for (i = 0; i<Size; i++)
				{
					target[i] = source[i];
				}

				//RtlCopyMemory(target, source, Size);

				//DbgPrint("%x %x", target, target[0]);
				__writecr0(__readcr0() | 0x10000);

				//	RtlCopyMemory(target, source, Size);
				ntStatus = STATUS_SUCCESS;



			}


		}
		__finally
		{
			KeDetachProcess();
		}
	}
	__except (1)
	{
		//DbgPrint("Error while writing\n");
		ntStatus = STATUS_UNSUCCESSFUL;
	}

	if (PEProcess == NULL) //no valid peprocess was given so I made a reference, so lets also dereference
		ObDereferenceObject(selectedprocess);

	return NT_SUCCESS(ntStatus);
}


BOOLEAN ReadProcessMemory(DWORD PID, PEPROCESS PEProcess, PVOID Address, DWORD Size, PVOID Buffer)
{
	PEPROCESS selectedprocess = PEProcess;
	//KAPC_STATE apc_state;
	NTSTATUS ntStatus = STATUS_UNSUCCESSFUL;

	if (PEProcess == NULL)
	{
		if (!NT_SUCCESS(PsLookupProcessByProcessId((PVOID)(UINT_PTR)PID, &selectedprocess)))
			return FALSE; //couldn't get the PID

	}

	//selectedprocess now holds a valid peprocess value
	__try
	{
		UINT_PTR temp = (UINT_PTR)Address;

		KeAttachProcess((PEPROCESS)selectedprocess);


		__try
		{
			char* target;
			char* source;
			int i;


			if ((IsAddressSafe((UINT_PTR)Address)) && (IsAddressSafe((UINT_PTR)Address + Size - 1)))
			{



				target = (char *)Buffer;
				source = (char *)Address;


				RtlCopyMemory(target, source, Size);

				ntStatus = STATUS_SUCCESS;


			}

		}
		__finally
		{

			KeDetachProcess();
		}
	}
	__except (1)
	{
		//DbgPrint("Error while reading: ReadProcessMemory(%x,%p, %p, %d, %p\n", PID, PEProcess, Address, Size, Buffer);

		ntStatus = STATUS_UNSUCCESSFUL;
	}

	if (PEProcess == NULL) //no valid peprocess was given so I made a reference, so lets also dereference
		ObDereferenceObject(selectedprocess);

	return NT_SUCCESS(ntStatus);



}


UINT_PTR getPEThread(UINT_PTR threadid)
{
	//UINT_PTR *threadid;
	PETHREAD selectedthread;
	UINT_PTR result = 0;



	if (PsLookupThreadByThreadId((PVOID)(UINT_PTR)threadid, &selectedthread) == STATUS_SUCCESS)
	{
		result = (UINT_PTR)selectedthread;
		ObDereferenceObject(selectedthread);
	}

	return result;
}


BOOLEAN IsAddressSafe(UINT_PTR StartAddress)
{
#ifdef AMD64
	//cannonical check. Bits 48 to 63 must match bit 47
	UINT_PTR toppart = (StartAddress >> 47);
	if (toppart & 1)
	{
		//toppart must be 0x1ffff
		if (toppart != 0x1ffff)
			return FALSE;
	}
	else
	{
		//toppart must be 0
		if (toppart != 0)
			return FALSE;

	}

#endif

	//return TRUE;



	{
#ifdef AMD64
		UINT_PTR kernelbase = 0x7fffffffffffffffULL;


		if (StartAddress<kernelbase)
			return TRUE;
		else
		{
			PHYSICAL_ADDRESS physical;
			physical.QuadPart = 0;
			physical = MmGetPhysicalAddress((PVOID)StartAddress);
			return (physical.QuadPart != 0);
		}



		return TRUE; //for now untill I ave figure out the win 4 paging scheme
#else
		/*	MDL x;


		MmProbeAndLockPages(&x,KernelMode,IoModifyAccess);


		MmUnlockPages(&x);
		*/
		ULONG kernelbase = 0x7ffe0000;

		if ((!HiddenDriver) && (StartAddress<kernelbase))
			return TRUE;

		{
			UINT_PTR PTE, PDE;
			struct PTEStruct *x;

			/*
			PHYSICAL_ADDRESS physical;
			physical=MmGetPhysicalAddress((PVOID)StartAddress);
			return (physical.QuadPart!=0);*/


			PTE = (UINT_PTR)StartAddress;
			PTE = PTE / 0x1000 * PTESize + 0xc0000000;

			//now check if the address in PTE is valid by checking the page table directory at 0xc0300000 (same location as CR3 btw)
			PDE = PTE / 0x1000 * PTESize + 0xc0000000; //same formula

			x = (PVOID)PDE;
			if ((x->P == 0) && (x->A2 == 0))
			{
				//Not present or paged, and since paging in this area isn't such a smart thing to do just skip it
				//perhaps this is only for the 4 mb pages, but those should never be paged out, so it should be 1
				//bah, I've got no idea what this is used for
				return FALSE;
			}

			if (x->PS == 1)
			{
				//This is a 4 MB page (no pte list)
				//so, (startaddress/0x400000*0x400000) till ((startaddress/0x400000*0x400000)+(0x400000-1) ) ) is specified by this page
			}
			else //if it's not a 4 MB page then check the PTE
			{
				//still here so the page table directory agreed that it is a usable page table entry
				x = (PVOID)PTE;
				if ((x->P == 0) && (x->A2 == 0))
					return FALSE; //see for explenation the part of the PDE
			}

			return TRUE;
		}
#endif
	}

}