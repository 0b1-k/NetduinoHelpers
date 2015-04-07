using System;
using System.IO;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.IO;
using SecretLabs.NETMF.Hardware.Netduino;
using netduino.helpers.Helpers;

namespace JSONParserTest {
    class Program {
        static void Main(string[] args) {
            KloutTestScore();
            KloutTestShow();
            KloutTestTopics();
            KloutTestInfluencedBy();
            KloutTestInfluencees();
            TestUnicodeCharacters();
            // Requires connecting an SD card reader
            //TestParsingFromStream();
        }

        public static void TestParsingFromStream() {
            Trace("TestParsingFromFileStream");

            StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
            
            var json = "{\"status\":200,\"users\":[{\"twitter_id\":\"29406026\",\"twitter_screen_name\":\"fabienroyer\",\"score\":{\"kscore\":40.8,\"slope\":0.11,\"description\":\"is effectively using social media to influence their network across a variety of topics\",\"kclass_id\":0,\"kclass\":\"Networker\",\"kclass_description\":\"You know how to connect to the right people and share what's important to your audience. You generously share your network to help your followers. You have a high level of engagement and an influential audience.\",\"kscore_description\":\"is effectively using social media to influence their network across a variety of topics\",\"network_score\":50.1,\"amplification_score\":19.24,\"true_reach\":71,\"delta_1day\":-0.02,\"delta_5day\":-1.4}},{\"twitter_id\":\"15602654\",\"twitter_screen_name\":\"bleroy\",\"score\":{\"kscore\":59.28,\"slope\":-0.02,\"description\":\"creates content that is spread throughout their network and drives discussions\",\"kclass_id\":11,\"kclass\":\"Specialist\",\"kclass_description\":\"You may not be a celebrity, but within your area of expertise your opinion is second to none. Your content is likely focused around a specific topic or industry with a focused, highly-engaged audience.\",\"kscore_description\":\"creates content that is spread throughout their network and drives discussions\",\"network_score\":66.85,\"amplification_score\":37.88,\"true_reach\":664,\"delta_1day\":0.33,\"delta_5day\":0.48}}]}";

            using (var file = new StreamWriter(@"SD\jsonTest.txt")) {
                file.WriteLine(json);
            }

            using (var file = new StreamReader(@"SD\jsonTest.txt")) {
                var parser = new JSONParser();
                var results = parser.Parse(file);

                short status;
                ArrayList users;

                if (parser.Find("status", results, out status)) {
                    if (status != 200) {
                        throw new Exception("200");
                    }                
                } else {
                    throw new Exception("status");
                }

                if (parser.Find("users", results, out users)) {
                    if (users.Count != 2) {
                        throw new Exception("users.Count");
                    }

                    foreach (Hashtable user in users) {
                        string screenName;
                        parser.Find("twitter_screen_name", user, out screenName);
                        Trace("Name: " + screenName);
                        DumpScoreDetails(user, parser);
                    }
                } else {
                    throw new Exception("users");
                }
            }

            StorageDevice.Unmount("SD");
        }

        public static void KloutTestInfluencees() {
            Trace("KloutTestInfluencees");

            var json = "{\"status\":200,\"users\":[{\"twitter_screen_name\":\"fabienroyer\",\"influencees\":[{\"twitter_screen_name\":\"freetheinternet\",\"kscore\":60.11},{\"twitter_screen_name\":\"bleroy\",\"kscore\":59.28}]},{\"twitter_screen_name\":\"bleroy\",\"influencees\":[{\"twitter_screen_name\":\"preetham_reddyc\",\"kscore\":24.68},{\"twitter_screen_name\":\"sharpgis\",\"kscore\":51.57},{\"twitter_screen_name\":\"danimeier\",\"kscore\":34.67},{\"twitter_screen_name\":\"unquale\",\"kscore\":37.36},{\"twitter_screen_name\":\"sfmskywalker\",\"kscore\":14.92}]}]}";
            var parser = new JSONParser();
            var results = parser.Parse(json);

            ArrayList users;
            parser.Find("users", results, out users);

            foreach (Hashtable user in users) {
                string screenName;
                parser.Find("twitter_screen_name", user, out screenName);
                Trace("Name: " + screenName);
                DumpInfluencees(user, parser);
            }
        }

        public static void DumpInfluencees(Hashtable user, JSONParser parser) {
            ArrayList influencees;
            parser.Find("influencees", user, out influencees);
            foreach (Hashtable influencer in influencees) {
                string twitter_screen_name;
                parser.Find("twitter_screen_name", influencer, out twitter_screen_name);
                Trace("     Influencee: " + twitter_screen_name);
            }
        }

        public static void KloutTestInfluencedBy() {
            Trace("KloutTestInfluencedBy");
            var json = "{\"status\":200,\"users\":[{\"twitter_screen_name\":\"fabienroyer\",\"influencers\":[{\"twitter_screen_name\":\"bleroy\",\"kscore\":59.28}]},{\"twitter_screen_name\":\"bleroy\",\"influencers\":[{\"twitter_screen_name\":\"kristofera\",\"kscore\":43.51},{\"twitter_screen_name\":\"bradygaster\",\"kscore\":42.19},{\"twitter_screen_name\":\"anthonysteele\",\"kscore\":47.8},{\"twitter_screen_name\":\"ardalis\",\"kscore\":55.07},{\"twitter_screen_name\":\"kevindente\",\"kscore\":61.72}]}]}";
            var parser = new JSONParser();
            var results = parser.Parse(json);

            ArrayList users;
            parser.Find("users", results, out users);

            foreach (Hashtable user in users) {
                string screenName;
                parser.Find("twitter_screen_name", user, out screenName);
                Trace("Name: " + screenName);
                DumpInfluencers(user, parser);
            }
        }

        public static void DumpInfluencers(Hashtable user, JSONParser parser) {
            ArrayList influencers;
            parser.Find("influencers", user, out influencers);
            foreach (Hashtable influencer in influencers) {
                string twitter_screen_name;
                parser.Find("twitter_screen_name", influencer, out twitter_screen_name);
                Trace("     Influencer: " + twitter_screen_name);
            }
        }

        public static void TestUnicodeCharacters() {
            Trace("TestUnicodeCharacter");

            var json = "{\"unicodeCharTest\":\"user\\u0040gmail.com\\u00AA\\u00BB\\u00CC\\u00DD\\u00EE\\u00FF\\u19AF\\u1234\\u5678\\ubeef\\ub00b\\u0bad\\uc001\\u1337\"}";
            var parser = new JSONParser();
            var results = parser.Parse(json);
            string unicodeCharTest;
            parser.Find("unicodeCharTest", results, out unicodeCharTest);
            Trace("unicodeCharTest: " + unicodeCharTest);
        }

        public static void KloutTestTopics() {
            Trace("KloutTestTopics");

            var json = "{\"status\":200,\"users\":[{\"twitter_screen_name\":\"fabienroyer\",\"topics\":[\"hardware\"]},{\"twitter_screen_name\":\"bleroy\",\"topics\":[\"sony\",\"sarah palin\",\"republican party\",\"algorithms\",\"physics\"]}]}";
            var parser = new JSONParser();
            var results = parser.Parse(json);

            short status;
            ArrayList users;

            parser.Find("status", results, out status);
            parser.Find("users", results, out users);

            if (status != 200 || users == null) {
                throw new Exception("KloutTestTopics");
            }

            if (users.Count != 2) {
                throw new Exception("KloutTestTopics");
            }

            foreach (Hashtable user in users) {
                string screenName;
                parser.Find("twitter_screen_name", user, out screenName);
                Trace("Name: " + screenName);
                DumpTopics(user, parser);
            }
        }

        public static void DumpTopics(Hashtable user, JSONParser parser) {
            ArrayList topics;
            parser.Find("topics", user, out topics);
            foreach (string topic in topics) {
                Trace("     Topic: " + topic);
            }
        }

        public static void KloutTestShow() {
            Trace("KloutTestShow");

            var json = "{\"status\":200,\"users\":[{\"twitter_id\":\"29406026\",\"twitter_screen_name\":\"fabienroyer\",\"score\":{\"kscore\":40.8,\"slope\":0.11,\"description\":\"is effectively using social media to influence their network across a variety of topics\",\"kclass_id\":0,\"kclass\":\"Networker\",\"kclass_description\":\"You know how to connect to the right people and share what's important to your audience. You generously share your network to help your followers. You have a high level of engagement and an influential audience.\",\"kscore_description\":\"is effectively using social media to influence their network across a variety of topics\",\"network_score\":50.1,\"amplification_score\":19.24,\"true_reach\":71,\"delta_1day\":-0.02,\"delta_5day\":-1.4}},{\"twitter_id\":\"15602654\",\"twitter_screen_name\":\"bleroy\",\"score\":{\"kscore\":59.28,\"slope\":-0.02,\"description\":\"creates content that is spread throughout their network and drives discussions\",\"kclass_id\":11,\"kclass\":\"Specialist\",\"kclass_description\":\"You may not be a celebrity, but within your area of expertise your opinion is second to none. Your content is likely focused around a specific topic or industry with a focused, highly-engaged audience.\",\"kscore_description\":\"creates content that is spread throughout their network and drives discussions\",\"network_score\":66.85,\"amplification_score\":37.88,\"true_reach\":664,\"delta_1day\":0.33,\"delta_5day\":0.48}}]}";
            var parser = new JSONParser();
            var results = parser.Parse(json);

            short status;
            ArrayList users;

            parser.Find("status", results, out status);
            parser.Find("users", results, out users);

            if (status != 200 || users == null) {
                throw new Exception("KloutTestShow");
            }

            if (users.Count != 2) {
                throw new Exception("KloutTestShow");
            }

            foreach (Hashtable user in users) {
                string screenName;
                parser.Find("twitter_screen_name", user, out screenName);
                Trace("Name: " + screenName); 
                DumpScoreDetails(user, parser);
            }
        }
        
        public static void DumpScoreDetails(Hashtable user, JSONParser parser) {
            Hashtable score;
                
            parser.Find("score", user, out score);

            Double delta_1day;
            float kscore;
            float network_score;
            float amplification_score;
            int kclass_id;
            short true_reach;
            float delta_5day;
            float slope;
            string kclass;

            parser.Find("delta_1day", score, out delta_1day);
            parser.Find("kscore", score, out kscore);
            parser.Find("network_score", score, out network_score);
            parser.Find("amplification_score", score, out amplification_score);
            parser.Find("kclass_id", score, out kclass_id);
            parser.Find("true_reach", score, out true_reach);
            parser.Find("delta_5day", score, out delta_5day);
            parser.Find("slope", score, out slope);
            parser.Find("kclass", score, out kclass);

            Trace("     delta_1day: " + delta_1day.ToString());
            Trace("     kscore: " + kscore.ToString());
            Trace("     network_score: " + network_score.ToString());
            Trace("     amplification_score: " + amplification_score.ToString());
            Trace("     kclass_id: " + kclass_id.ToString());
            Trace("     true_reach: " + true_reach.ToString());
            Trace("     delta_5day: " + delta_5day.ToString());
            Trace("     slope: " + slope.ToString());
            Trace("     kclass: " + kclass.ToString());
        }

        public static void KloutTestScore() {
            Trace("KloutTestScore");

            var json = "{\"status\":200,\"users\":[{\"twitter_screen_name\":\"fabienroyer\",\"kscore\":40.8},{\"twitter_screen_name\":\"bleroy\",\"kscore\":59.28}]}";
            var parser = new JSONParser();

            var results = parser.Parse(json);

            short status;
            ArrayList users;

            parser.Find("status", results, out status);
            parser.Find("users", results, out users);

            if (status != 200 || users == null) {
                throw new Exception("KloutTestScore");
            }

            foreach (Hashtable user in users) {
                string screenName;
                float kscore;
                parser.Find("twitter_screen_name", user, out screenName);
                parser.Find("kscore", user, out kscore);
                Trace("Name: " + screenName + ", kscore: " + kscore.ToString());
            }
        }

        public static void Trace(string data) {
            Debug.Print(data);
        }
    }
}
