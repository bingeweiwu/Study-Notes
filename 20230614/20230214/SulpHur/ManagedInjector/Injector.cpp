// Copyright © 2006 Microsoft Corporation.  All Rights Reserved

#include "stdafx.h"

#include "Injector.h"
#include <vcclr.h>

using namespace ManagedInjector;

static unsigned int WM_GOBABYGO = ::RegisterWindowMessage(L"Injector_GOBABYGO!");
static HHOOK _messageHookHandle;

System::String^ Injector::GetControlType(System::IntPtr windowHandle, System::Reflection::Assembly^ assembly, System::String^ className, System::String^ methodName) {

	System::String^ assemblyClassAndMethod = assembly->Location + "$" + className + "$" + methodName;
	pin_ptr<const wchar_t> acmLocal = PtrToStringChars(assemblyClassAndMethod);

	HINSTANCE hinstDLL;
	if(System::Environment::Is64BitProcess == TRUE)
	{
		hinstDLL = ::LoadLibrary((LPCTSTR) _T("ManagedInjector_x64.dll")); 
	} 
	else
	{
		hinstDLL = ::LoadLibrary((LPCTSTR) _T("ManagedInjector_x86.dll")); 
	}

	wchar_t* temp = new wchar_t[0x4000];

	if (hinstDLL)
	{
		DWORD processID = 0;
		DWORD threadID = ::GetWindowThreadProcessId((HWND)windowHandle.ToPointer(), &processID);

		if (processID)
		{
			HANDLE hProcess = ::OpenProcess(PROCESS_ALL_ACCESS, FALSE, processID);
			if (hProcess)
			{
				int buffLen = (assemblyClassAndMethod->Length + 1) * sizeof(wchar_t);
				void* acmRemote = ::VirtualAllocEx(hProcess, NULL, 0x4000, MEM_COMMIT, PAGE_READWRITE);

				if (acmRemote)
				{
					::WriteProcessMemory(hProcess, acmRemote, acmLocal, buffLen, NULL);
				
					HOOKPROC procAddress = (HOOKPROC)GetProcAddress(hinstDLL, "MessageHookProc");
					_messageHookHandle = ::SetWindowsHookEx(WH_CALLWNDPROC, procAddress, hinstDLL, threadID);

					if (_messageHookHandle)
					{
						::SendMessage((HWND)windowHandle.ToPointer(), WM_GOBABYGO, (WPARAM)acmRemote, 0);
						::UnhookWindowsHookEx(_messageHookHandle);
					}

					SIZE_T lpNumberOfBytesRead = 0;
					
					::ReadProcessMemory(hProcess, acmRemote, temp, 0x4000, &lpNumberOfBytesRead);
					::VirtualFreeEx(hProcess, acmRemote, 0, MEM_RELEASE);
				}

				::CloseHandle(hProcess);
			}
		}
		::FreeLibrary(hinstDLL);

		return gcnew System::String(temp);
	}

	return System::String::Empty;
}

__declspec( dllexport ) 
LRESULT __stdcall MessageHookProc(int nCode, WPARAM wparam, LPARAM lparam) {

	if (nCode == HC_ACTION) 
	{
		CWPSTRUCT* msg = (CWPSTRUCT*)lparam;
		if (msg != NULL && msg->message == WM_GOBABYGO)
		{
			wchar_t* acmRemote = (wchar_t*)msg->wParam;

			System::String^ acmLocal = gcnew System::String(acmRemote);
			cli::array<System::String^>^ acmSplit = acmLocal->Split('$');

			System::Reflection::Assembly^ assembly = System::Reflection::Assembly::LoadFrom(acmSplit[0]);
			if (assembly != nullptr)
			{
				System::Type^ type = assembly->GetType(acmSplit[1]);
				if (type != nullptr)
				{
					System::Reflection::MethodInfo^ methodInfo = type->GetMethod(acmSplit[2], System::Reflection::BindingFlags::Static | System::Reflection::BindingFlags::Public);
					if (methodInfo != nullptr)
					{
						cli::array<System::IntPtr^>^ paramArray = { (System::IntPtr)msg->hwnd };
						
						System::String^ result = (System::String^)methodInfo->Invoke(nullptr, paramArray);

						if(System::String::IsNullOrEmpty(result) == FALSE)
						{
							DWORD processID = 0;
							DWORD threadID = ::GetWindowThreadProcessId(msg->hwnd, &processID);

							if (processID)
							{
								HANDLE hProcess = ::OpenProcess(PROCESS_ALL_ACCESS, FALSE, processID);
								if (hProcess)
								{
									::VirtualAllocEx(hProcess, acmRemote, 0x4000, MEM_DECOMMIT, PAGE_READWRITE);
									::VirtualAllocEx(hProcess, acmRemote, 0x4000, MEM_COMMIT, PAGE_READWRITE);
									int buffLen = (result->Length + 1) * sizeof(wchar_t);

									pin_ptr<const wchar_t> temp = PtrToStringChars(result);

									::WriteProcessMemory(hProcess, acmRemote, temp, buffLen, NULL);

									::CloseHandle(hProcess);
								}
							}
						}
					}
				}
			}
		}
	}
	return CallNextHookEx(_messageHookHandle, nCode, wparam, lparam);
}