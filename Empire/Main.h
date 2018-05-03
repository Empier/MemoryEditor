#ifndef __MAIN__
#define __MAIN__ 1

#include <ntifs.h>
#include <ntddk.h>
#include <intrin.h>
//#include <wdf.h>  
//#include <ndis.h>            /// Inc
#include <ntintsafe.h>       /// Inc
//#include <ntstrsafe.h>       /// Inc
#include "memory_module.h"
#define IOCTL_READ_MEMORY CTL_CODE(FILE_DEVICE_UNKNOWN, 0x2000, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS)
#define IOCTL_WRITE_MEMORY CTL_CODE(FILE_DEVICE_UNKNOWN, 0x2001, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS)
#define IOCTL_OPEN_PROCESS CTL_CODE(FILE_DEVICE_UNKNOWN, 0x2002, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS)




#endif // !__MAIN__


