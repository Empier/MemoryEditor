


BOOLEAN IsAddressSafe(UINT_PTR StartAddress);
UINT_PTR getPEThread(UINT_PTR threadid);
BOOLEAN WriteProcessMemory(DWORD PID, PEPROCESS PEProcess, PVOID Address, DWORD Size, PVOID Buffer);
BOOLEAN ReadProcessMemory(DWORD PID, PEPROCESS PEProcess, PVOID Address, DWORD Size, PVOID Buffer);
