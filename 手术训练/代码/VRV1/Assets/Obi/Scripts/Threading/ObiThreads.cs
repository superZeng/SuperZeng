using UnityEngine;
using System.Collections;
using System.Threading;

namespace Obi{

	public static class ObiThreads
	{

		public static void GetArrayStartEndForThread(int count, int threadCount, int threadIndex, out int start, out int end){
			int ppt = Mathf.CeilToInt(count / (float)threadCount);
			start = ppt * threadIndex;
			end = Mathf.Min (count,ppt*(threadIndex+1));
		}
	
		public static void DoTask(int numThreads, System.Action<int,int,object> task, object state){

			// If there's no task to do, return.
			if (task == null) return;

			// In case the caller wants 0 worker threads, do this on the main thread.
			if (numThreads == 0){
				task(1,0,state);
				return;
			}

			// Initialize a counter to know when all pool threads have finished.
			int counter = numThreads;

			using(ManualResetEvent resetEvent = new ManualResetEvent(false))
			{
				for (int i = 0; i < numThreads; i++){

					int threadIndex = i;

					ThreadPool.QueueUserWorkItem((object threadState) => {
						
						task(numThreads,threadIndex,threadState);
						
						if (Interlocked.Decrement(ref counter) == 0)
							resetEvent.Set();
						
					},state);
				}
				
				resetEvent.WaitOne();
			}

		}		

	}

}

