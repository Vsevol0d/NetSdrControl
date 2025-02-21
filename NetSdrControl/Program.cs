using NetSdrControl.ControlItems;
using NetSdrControl.DataItems;
using NetSdrControl.Protocol;
using NetSdrControl.Protocol.HelpMessages;
using NetSdrControlTests.UDP;

namespace NetSdrControl
{
    public class Program
    {
        public static void Main()
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            Console.WriteLine("NetSdr Control Application");
            Console.WriteLine("Available commands:");
            Console.WriteLine("-connect <ip address>");
            Console.WriteLine("-change_freq <channel(1, 2, all)> <center frequency(MHz)>");
            Console.WriteLine("-get_freq <channel(1, 2, all)>");
            Console.WriteLine("-start_IQ <sample type(real, iq)> <capture mode(16, 24)> <capture way(contiguous, fifo, hardware)> <samples count(only for FIFO)>");
            Console.WriteLine("-stop_IQ");
            Console.WriteLine("-disconnect");
            Console.WriteLine("-Esc - exit");

            var bitDecoder = new BitDecoder();
            var controlItemHeader = new ControlItemHeader();
            var messageBuilder = new MessageBuilder(bitDecoder, controlItemHeader, new NAKMessage());

            var client = new NetSdrTcpClient<DefaultTcpClient>(new NetSdrTcpClientSettings(), new DefaultTcpClient(), bitDecoder, controlItemHeader);
            var frequencyControl = new ReceiverFrequencyControlItem(client, messageBuilder, bitDecoder);
            var dataExchanger = new DataExchanger(new NetSdrUdpClient<DefaultUdpClient>(new DefaultUdpClient(), new NetSdrUdpClientSettings()), new FileSystem(new FileSystemSettings()));
            var receiverState = new ReceiverStateItem(client, messageBuilder, bitDecoder, dataExchanger);

            var keyInfo = Console.ReadKey().Key;
            while (keyInfo != ConsoleKey.Escape)
            {
                var command = Console.ReadLine();
                if (string.IsNullOrEmpty(command))
                {
                    continue;
                }

                var commandParams = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var commandName = keyInfo.ToString().ToLower() + commandParams.FirstOrDefault();
                if (string.IsNullOrEmpty(commandName))
                {
                    Console.WriteLine("Command name is wrong");
                    continue;
                }

                try
                {
                    switch (commandName)
                    {
                        case "connect":
                            await client.Connect(commandParams[1]);
                            break;
                        case "disconnect":
                            client.Disconnect();
                            break;
                        case "get_freq":
                            await frequencyControl.GetFrequency(commandParams[1] switch
                            {
                                "1" => FrequencyChannel.Channel_1,
                                "2" => FrequencyChannel.Channel_2,
                                "all" => FrequencyChannel.All,
                                _ => throw new NotImplementedException()
                            });
                            break;
                        case "set_freq":
                            await frequencyControl.ChangeFrequency(commandParams[1] switch
                            {
                                "1" => FrequencyChannel.Channel_1,
                                "2" => FrequencyChannel.Channel_2,
                                "all" => FrequencyChannel.All,
                                _ => throw new NotImplementedException()
                            }, Convert.ToUInt64(commandParams[2]));
                            break;
                        case "start_IQ":
                            await receiverState.StartIQTransfer(commandParams[1] switch
                            {
                                "real" => NetSdrDataMode.RealADSample,
                                "iq" => NetSdrDataMode.ComplexIQBaseBand,
                                _ => throw new NotImplementedException()
                            },
                                commandParams[2] switch
                                {
                                    "16" => NetSdrCaptureMode.x_16,
                                    "24" => NetSdrCaptureMode.x_24,
                                    _ => throw new NotImplementedException()
                                }, commandParams[3] switch
                                {
                                    "contiguous" => NetSdrCaptureWay.Contiguous,
                                    "fifo" => NetSdrCaptureWay.FIFO,
                                    "hardware" => NetSdrCaptureWay.Hardware,
                                    _ => throw new NotImplementedException()
                                }, Convert.ToByte(commandParams[3]));
                            break;
                        case "stop_IQ":
                            await receiverState.StopIQTransfer();
                            break;
                        default: 
                            Console.WriteLine("Command was not recognized");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Command failed:");
                    Console.WriteLine(ex.Message);
                }
                keyInfo = Console.ReadKey().Key;
            }
        }
    }
}