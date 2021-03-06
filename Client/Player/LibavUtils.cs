﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

namespace SysDVR.Client.Player
{
	public static class LibavUtils
	{
		public record Codec(string Name, string Description, AVPixelFormat[] Formats);

		public static void PrintCpuArchWarning() 
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !Environment.Is64BitProcess)
			{
				Console.WriteLine(
					"WARNING: You seem to be running the 32-bit version of .NET, on Windows this is NOT supported due to ffmpeg not providing official 32-bit versions of their libs.\r\n" +
					"If you're running a 64-bit install of Windows (check your pc info) uninstall .NET and install the x64 version from Microsoft's website.\r\n" +
					"If you're running a 32-bit install of Windows you should upgrade your PC. Alternatively you need to find a 32-bit build of ffmpeg libs and copy them in the SysDVR-Client folder after that you can ignore this warning.\r\n" +
					"Continuing without the 32-bit libs WILL crash with a missing libraries/wrong library format error."
				);
			}
		}

		public static unsafe Codec[] GetAllDecoders()
		{
			List<Codec> codecs = new List<Codec>();

			void* i = null;
			AVCodec* c = null;

			while ((c = av_codec_iterate(&i)) != null)
			{
				if (av_codec_is_decoder(c) != 0)
				{
					var desc = Marshal.PtrToStringAnsi((IntPtr)c->long_name);
					var name = Marshal.PtrToStringAnsi((IntPtr)c->name);

					List<AVPixelFormat> formats = new List<AVPixelFormat>();
					var fmt = c->pix_fmts;
					while (fmt != null && *fmt != AVPixelFormat.AV_PIX_FMT_NONE)
					{
						formats.Add(*fmt);
						fmt++;
					}

					codecs.Add(new Codec(name, desc, formats.ToArray()));
				}
			}

			return codecs.ToArray();
		}

		public static IEnumerable<Codec> GetH264Decoders() =>
			GetAllDecoders().Where(x => x.Name.Contains("264") || x.Description.Contains("264"));

		public static void PrintAllCodecs() 
		{
			Console.WriteLine("List of all compatible video decoders: ");
			foreach (var c in GetH264Decoders())
				Console.WriteLine($"{c.Name} : {c.Description} - Formats: [{string.Join(',', c.Formats)}]");
		}
		
	}
}
