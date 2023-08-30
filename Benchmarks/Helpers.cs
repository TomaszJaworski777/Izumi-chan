using System.Runtime.CompilerServices;

#pragma warning disable CS8500

namespace Benchmarks;

public static unsafe class Helpers
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static T? Use<T>(T? value) => value;
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static ref T? Use<T>(ref T? value) => ref value;
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static T?* Use<T>(T?* value) => value;
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static void* Use(void* value) => value;
}