using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using WaybackMachine.DotNet.Client;
//using WaybackDownloader;

namespace WaybackDownloader
{
	class Program
	{
		static async Task Main(string[] args)
		{
			//If this is not done, then the Windows-1252 encoding used by the PL pages will throw a fit
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			var urls = new List<string>()
			{
				"http://img-nex.theonering.net/*"
				//"http://eccentric.com/*",

				//"http://decktech.net/*",
				//"http://decktech.net:80/*",
				//"http://www.decktech.net./*",
				//"http://www.decktech.net/*",
				//"http://www.decktech.net:80/*",
				//"http://www1.decktech.net/*",
				//"http://www1.decktech.net:80/*",

				//"http://www.decipher.com:80/*",
				//"http://archive.decipher.com/*",
				//"http://www.archive.decipher.com/*",
				//"http://archives.decipher.com/*",
				//"http://www.archives.decipher.com/*",
				//"http://bbs.decipher.com/*",
				//"http://calder.decipher.com/*",
				//"https://calder.decipher.com/*",
				//"http://calder.decipher.com:8080",
				//"http://christo.decipher.com/*",
				//"http://christo.decipher.com./*",
				//"http://cipher-ctf.decipher.com:80/*",
				//"http://www.msn.comv2.decipher.com:80/*",
				//"http://dali.decipher.com:80/*",
				//"http://dali.decipher.com/*",
				//"http://ebayiframe.decipher.com/*",
				//"http://ebayiframe.decipher.com:80/*",
				//"http://www.ebayiframe.decipher.com/*",
				//"http://fanhq.decipher.com",
				//"http://fanhq.decipher.com/*",
				//"http://forums.decipher.com/*",
				//"http://www.forums.decipher.com/*",
				//"http://ftp.decipher.com/*",
				//"http://games.decipher.com:80/*",
				//"http://games.decipher.com/*",
				//"http://www.games.decipher.com/*",
				//"http://www.howtohost.decipher.com:80/*",
				//"http://howtohost.decipher.com/*",
				//"http://http.decipher.com/*",
				//"http://www.http.decipher.com/*",
				//"http://info.decipher.com:80/*",
				//"http://info.decipher.com/*",
				//"http://www.info.decipher.com/*",
				//"http://lists.decipher.com:80/*",
				//"http://lists.decipher.com/*",
				//"http://lotr.decipher.com/*",
				//"http://lotr.decipher.com:80/*",
				//"http://www.lotr.decipher.com/*",
				//"http://lotronline.decipher.com:80/*",
				//"https://lotronline.decipher.com/*",
				//"http://lotronline.decipher.com/*",
				//"https://lotronline.decipher.com./*",
				//"http://www.lotronline.decipher.com/*",
				//"https://www.lotronline.decipher.com/*",
				//"http://lotrrp.decipher.com/*",
				//"http://lotrrpg.decipher.com:80/*",
				//"http://lotrrpg.decipher.com/*",
				//"http://www.lotrrpg.decipher.com/*",
				//"http://lotrtcg.decipher.com:80/*",
				//"http://lotrtcg.decipher.com/*",
				//"http://www.lotrtcg.decipher.com/*",
				//"http://marajade.decipher.com:80/*",
				//"http://marajade.decipher.com/*",
				//"http://megamantcg.decipher.com/*",
				//"http://ns1.decipher.com/*",
				//"http://ns2.decipher.com/*",
				//"http://online.decipher.com:80/*",
				//"http://online.decipher.com/*",
				//"http://www.online.decipher.com/*",
				//"http://p.decipher.com/*",
				//"http://partyzone.decipher.com:80/*",
				//"http://pente.decipher.com:80/*",
				//"http://pente.decipher.com/*",
				//"http://www.pente.decipher.com/*",
				//"http://pollock.decipher.com:80/*",
				//"http://pollock.decipher.com/*",
				//"http://redirect.decipher.com:80/*",
				//"http://reinvent.decipher.com:80/*",
				//"http://reinvent.decipher.com/*",
				//"http://www.reinvent.decipher.com/*",
				//"http://retailers.decipher.com:80/*",
				//"http://retailers.decipher.com/*",
				//"http://www.retailers.decipher.com/*",
				//"http://rewards.decipher.com:80/*",
				//"http://rewards.decipher.com/*",
				//"http://www.rewards.decipher.com/*",
				//"http://rothko.decipher.com:80/*",
				//"http://rothko.decipher.com/*",
				//"http://search.decipher.com:80/*",
				//"http://search.decipher.com/*",
				//"http://shop.decipher.com:80/*",
				//"http://www.shop.decipher.com:80/*",
				//"http://shop.decipher.com/*",
				//"http://www.shop.decipher.com/*",
				//"https://shop.decipher.com/*",
				//"http://startrekccg.decipher.com/",

				//"http://www.fanhq.com:80/*",
				//"http://www.20dollarbag.fanhq.com:80/*",
				//"http://20dollarbag.fanhq.com/*",
				//"http://batman.fanhq.com:80/*",
				//"http://batman.fanhq.com/*",
				//"http://www.batman.fanhq.com/*",
				//"http://battlebands.fanhq.com/*",
				//"http://beybladetcg.fanhq.com/*",
				//"http://www.beybladetcg.fanhq.com/*",
				//"http://boycrazy.fanhq.com/*",
				//"http://www.boycrazy.fanhq.com/*",
				//"http://buffy.fanhq.com:80/*",
				//"http://buffy.fanhq.com/*",
				//"http://coca-cola.fanhq.com/*",
				//"http://www.coca-cola.fanhq.com/*",
				//"http://comingattractions.fanhq.com/*",
				//"http://www.comingattractions.fanhq.com/*",
				//"http://deciper.fanhq.com/*",
				//"http://www.deciper.fanhq.com/*",
				//"http://decipher.fanhq.com:80/*",
				//"http://decipher.fanhq.com/*",
				//"http://www.decipher.fanhq.com:80/*",
				//"http://www.decipher.fanhq.com/*",
				//"https://decipher.fanhq.com/*",
				//"http://decipherstore.fanhq.com/*",
				//"http://www.decipherstore.fanhq.com/*",
				//"https://decipherstore.fanhq.com/*",
				//"http://disney.fanhq.com:80/*",
				//"http://disney.fanhq.com/*",
				//"http://www.disney.fanhq.com/*",
				//"http://dothackenemy.fanhq.com:80/*",
				//"http://dothackenemy.fanhq.com/*",
				//"http://www.dothackenemy.fanhq.com/*",
				//"https://dothackenemy.fanhq.com/*",
				//"http://e.fanhq.com/*",



				//"http://forums.fanhq.com:80/*",
				//"http://forums.fanhq.com/*",
				//"https://forums.fanhq.com/*",
				//"http://howtohost.fanhq.com:80/*",
				//"http://howtohost.fanhq.com/*",
				//"http://www.howtohost.fanhq.com/*",
				//"http://lordoftheringstcg.fanhq.com:80/*",
				//"http://lordoftheringstcg.fanhq.com/*",
				//"http://lordoftheringtcg.fanhq.com/*",
				//"http://www.lordoftheringtcg.fanhq.com/*",
				//"http://lotr.fanhq.com:80/*",
				//"http://lotr.fanhq.com/*",
				//"http://lotr.fanhq.com./*",
				//"http://www.lotr.fanhq.com/*",
				//"https://lotr.fanhq.com/*",
				//"http://lotrrpg.fanhq.com:80/*",
				//"http://lotrrpg.fanhq.com/*",
				//"http://www.lotrrpg.fanhq.com/*",
				//"https://lotrrpg.fanhq.com/*",
				//"http://lotrtcg.fanhq.com:80/*",
				//"http://lotrtcg.fanhq.com/*",
				//"https://lotrtcg.fanhq.com/*",
				//"http://www.lotrtcg.fanhq.com/*",
				//"http://megamantc.fanhq.com/*",
				//"http://www.megamantc.fanhq.com/*",
				//"http://megamantcg.fanhq.com:80/*",
				//"http://megamantcg.fanhq.com/*",
				//"http://www.megamantcg.fanhq.com/*",
				//"https://megamantcg.fanhq.com/*",
				//"http://retailers.fanhq.com:80/*",
				//"http://retailers.fanhq.com/*",
				//"http://www.retailers.fanhq.com/*",
				//"http://rewards.fanhq.com:80/*",
				//"http://rewards.fanhq.com/*",
				//"http://www.rewards.fanhq.com/*",
				//"http://secure.fanhq.com:80/*",
				//"http://secure.fanhq.com/*",
				//"https://secure.fanhq.com/*",
				//"http://www.secure.fanhq.com/*",
				//"http://shufflepods.fanhq.com:80/*",
				//"http://shufflepods.fanhq.com/*",
				//"http://simpsons.fanhq.com:80/*",
				//"http://simpsons.fanhq.com/*",
				//"http://www.simpsons.fanhq.com/*",
				//"http://startrek.fanhq.com:80/*",
				//"http://startrek.fanhq.com/*",
				//"http://www.startrek.fanhq.com/*",
				//"http://startrekccg.fanhq.com:80/*",
				//"http://startrekccg.fanhq.com/*",
				//"https://startrekccg.fanhq.com/*",
				//"http://www.startrekccg.fanhq.com/*",
				//"http://startrekrpg.fanhq.com:80/*",
				//"http://startrekrpg.fanhq.com/*",
				//"http://www.startrekrpg.fanhq.com/*",
				//"http://starwars.fanhq.com:80/*",
				//"http://starwars.fanhq.com/*",
				//"http://www.starwars.fanhq.com/*",
				//"http://thankyougeorge.fanhq.com:80/*",
				//"http://thankyougeorge.fanhq.com/*",
				//"http://www.thankyougeorge.fanhq.com/*",
				//"http://things.fanhq.com/*",
				//"http://things.fanhq.com:80/*",
				//"http://totrtcg.fanhq.com/*",
				//"http://volunteers.fanhq.com:80/*",
				//"http://volunteers.fanhq.com/*",
				//"http://www.volunteers.fanhq.com/*",
				//"http://wars.fanhq.com/*",
				//"http://www.wars.fanhq.com/*",
				//"http://warstcg.fanhq.com:80/*",
				//"http://warstcg.fanhq.com/*",
				//"http://www.warstcg.fanhq.com:80/*",
				//"http://www.warstcg.fanhq.com/*",
				//"https://warstcg.fanhq.com/*",


				////"http://decipher.com/*",
				////"http://www.decipher.com/*",
				////"http://lotrtcg.decipher.com/*",
				////"http://www.lotrtcg.decipher.com/*",
				////"http://shop.decipher.com/*",
				////"http://www.shop.decipher.com/*",

				////"http://fanhq.com/*",
				////"http://www.fanhq.com/*",
				////"http://decipher.fanhq.com/*",
				////"http://www.decipher.fanhq.com/*",
				////"http://decipherstore.fanhq.com/*",
				////"http://www.decipherstore.fanhq.com/*",
				////"http://lotrtcg.fanhq.com/*",
				////"http://www.lotrtcg.fanhq.com/*",

				////"http://dgma.com/*",
				////"http://www.dgma.com/*",

				////"http://lotrtcg.fanhq.com/Resources/CardImages/*",
				////"http://www.decipher.com/lordoftherings/cardlists*",
				////"http://shop.decipher.com/Images/*",
				////"http://decipher.fanhq.com/Resources/CardImages/*",
				////"http://www.decipher.fanhq.com/Resources/CardImages/*",

				////"http://www.decipher.com/lordoftherings/*",
				////"http://lotrtcg.fanhq.com/Resources/*",
				////"http://www.lotrtcg.fanhq.com/Resources/*",
				////"http://decipher.fanhq.com/Resources/*",
				////"http://www.decipher.fanhq.com/Resources/*",



			};

			try
			{
				var cdx = new CDXAccessor();

				foreach(var url in urls)
				{
					await cdx.FetchAllFilesFromURL(url, "output");
					//await cdx.FetchAllFileInfoFromURL(url, "output");
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
				Console.ReadKey();
			}
		}
	}
}
