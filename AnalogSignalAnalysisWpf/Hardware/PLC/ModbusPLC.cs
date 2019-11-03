﻿using NModbus;
using NModbus.Serial;
using System;
using System.IO.Ports;
using System.Linq;

namespace AnalogSignalAnalysisWpf.Hardware
{
    class ModbusPLC : IPLC
    {
        #region 构造函数

        /// <summary>
        /// 创建ModbusPower新实例
        /// </summary>
        public ModbusPLC() : this("")
        {

        }

        /// <summary>
        /// 创建ModbusPower新实例
        /// </summary>
        /// <param name="portName">端口名</param>
        /// <param name="baudRate">波特率</param>
        public ModbusPLC(string portName, int baudRate = 115200)
        {
            PrimarySerialPortName = portName;
            SerialPortBaudRate = baudRate;
        }

        #endregion

        #region Modbus接口

        private object powerLock = new object();

        /// <summary>
        /// 写单个寄存器
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="registerAddress">寄存器地址</param>
        /// <param name="value">数据</param>
        private void ModbusSerialRtuMasterWriteRegister(byte slaveAddress, ushort registerAddress, ushort value)
        {
            using (SerialPort port = new SerialPort(PrimarySerialPortName))
            {
                //配置串口
                port.BaudRate = SerialPortBaudRate;
                port.DataBits = 8;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.Open();

                //创建Modbus主机
                var adapter = new SerialPortAdapter(port);
                adapter.ReadTimeout = ReadTimeout;
                adapter.WriteTimeout = WriteTimeout;
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateRtuMaster(adapter);

                lock (powerLock)
                {
                    //写到寄存器
                    master.WriteSingleRegister(slaveAddress, registerAddress, value);
                }

            }
        }

        /// <summary>
        /// 写多个寄存器
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="registerAddress">寄存器地址</param>
        /// <param name="data">数据</param>
        private void ModbusSerialRtuMasterWriteRegister(byte slaveAddress, ushort registerAddress, ushort[] data)
        {
            using (SerialPort port = new SerialPort(PrimarySerialPortName))
            {
                //配置串口
                port.BaudRate = SerialPortBaudRate;
                port.DataBits = 8;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.Open();

                //创建Modbus主机
                var adapter = new SerialPortAdapter(port);
                adapter.ReadTimeout = ReadTimeout;
                adapter.WriteTimeout = WriteTimeout;
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateRtuMaster(adapter);

                lock (powerLock)
                {
                    //写到寄存器
                    master.WriteMultipleRegisters(slaveAddress, registerAddress, data);
                }
            }
        }

        /// <summary>
        /// 读单个寄存器
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="registerAddress">寄存器地址</param>
        /// <param name="value">数据</param>
        private void ModbusSerialRtuMasterReadRegister(byte slaveAddress, ushort registerAddress, out ushort value)
        {
            value = 0xFFFF;
            using (SerialPort port = new SerialPort(PrimarySerialPortName))
            {
                //配置串口
                port.BaudRate = SerialPortBaudRate;
                port.DataBits = 8;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.Open();

                //创建Modbus主机
                var adapter = new SerialPortAdapter(port);
                adapter.ReadTimeout = ReadTimeout;
                adapter.WriteTimeout = WriteTimeout;
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateRtuMaster(adapter);

                lock (powerLock)
                {
                    //读寄存器
                    var values = master.ReadHoldingRegisters(slaveAddress, registerAddress, 1);
                    if (values?.Length >= 1)
                    {
                        value = values[0];
                    }
                }
            }
        }

        /// <summary>
        /// 读多个寄存器
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="registerAddress">寄存器地址</param>
        /// <param name="value">数据</param>
        private void ModbusSerialRtuMasterReadRegister(byte slaveAddress, ushort registerAddress, ushort numberOfPoints, out ushort[] data)
        {
            data = null;
            using (SerialPort port = new SerialPort(PrimarySerialPortName))
            {
                //配置串口
                port.BaudRate = SerialPortBaudRate;
                port.DataBits = 8;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.Open();

                //创建Modbus主机
                var adapter = new SerialPortAdapter(port);
                adapter.ReadTimeout = ReadTimeout;
                adapter.WriteTimeout = WriteTimeout;
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateRtuMaster(adapter);

                lock (powerLock)
                {
                    //读寄存器
                    data = master.ReadHoldingRegisters(slaveAddress, registerAddress, numberOfPoints);
                }

            }
        }

        #endregion

        #region Modbus配置参数

        /// <summary>
        /// 串口号
        /// </summary>
        public string PrimarySerialPortName { get; set; } = "COM1";

        /// <summary>
        /// 串口波特率
        /// </summary>
        public int SerialPortBaudRate { get; set; } = 115200;

        /// <summary>
        /// 从站地址
        /// </summary>
        public byte SlaveAddress { get; set; } = 0x02;

        /// <summary>
        /// 写超时
        /// </summary>
        public int WriteTimeout { get; set; } = 500;

        /// <summary>
        /// 读超时
        /// </summary>
        public int ReadTimeout { get; set; } = 500;

        #endregion

        #region Modbus控制接口

        /// <summary>
        /// 设备连接标志
        /// </summary>
        public bool IsConnect { get; private set; } = false;

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <returns>执行结果</returns>
        public bool Connect()
        {
            if (string.IsNullOrEmpty(PrimarySerialPortName))
            {
                return false;
            }

            var Serials = SerialPort.GetPortNames();

            if (!Serials.Contains(PrimarySerialPortName))
            {
                return false;
            }

            ushort[] data;
            if (Read(IsEnablePulseAddress, 1, out data))
            {
                IsConnect = true;
                return true;
            }
            else
            {
                IsConnect = false;
                return false;
            }

        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            IsConnect = false;
        }

        /// <summary>
        /// 写参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="value">数值</param>
        /// <returns>执行结果</returns>
        public bool Write(ushort register, ushort value)
        {
            try
            {
                ModbusSerialRtuMasterWriteRegister(SlaveAddress, register, value);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 写参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="values">数值数值</param>
        /// <returns>执行结果</returns>
        public bool Write(ushort register, ushort[] values)
        {
            try
            {
                ModbusSerialRtuMasterWriteRegister(SlaveAddress, register, values);
            }
            catch (Exception)
            {
                return false;
            }
            return true;

        }

        /// <summary>
        /// 读参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="value">数值</param>
        /// <returns>执行结果</returns>
        public bool Read(ushort register, out ushort value)
        {
            value = 0xFFFF;

            try
            {
                ModbusSerialRtuMasterReadRegister(SlaveAddress, register, out value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 读参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="values">数值数值</param>
        /// <returns>执行结果</returns>
        public bool Read(ushort register, ushort count, out ushort[] values)
        {
            values = new ushort[0];

            try
            {
                ModbusSerialRtuMasterReadRegister(SlaveAddress, register, count, out values);
            }
            catch (Exception)
            {
                return false;
            }
            return true;

        }

        #endregion

        #region 控制接口

        #region 地址定义

        /// <summary>
        /// 使能脉冲地址
        /// </summary>
        private readonly ushort IsEnablePulseAddress = 0x0002;

        /// <summary>
        /// 频率地址
        /// </summary>
        private readonly ushort FrequencyAddress = 0x0001;

        /// <summary>
        /// 占空比
        /// </summary>
        private readonly ushort DutyRatioAddress = 0x0002;

        /// <summary>
        /// 开关状态地址
        /// </summary>
        private readonly ushort SwitchStatusAddress = 0x0003;

        #endregion

        /// <summary>
        /// 使能输出脉冲
        /// </summary>
        public bool IsEnablePulse
        {
            get
            {
                if (IsConnect)
                {
                    ushort[] data;
                    if (Read(IsEnablePulseAddress, 1, out data))
                    {
                        return data[0] != 0;
                    }
                }
                return false;
            }
            set
            {
                if (IsConnect)
                {
                    ushort data = value ? (ushort)1 : (ushort)0;
                    Write(IsEnablePulseAddress, data);
                }
            }
        }

        /// <summary>
        /// 频率(Hz)
        /// </summary>
        public int Frequency
        {
            get
            {
                if (IsConnect)
                {
                    ushort[] data;
                    if (Read(FrequencyAddress, 1, out data))
                    {
                        return data[0];
                    }
                }
                return -1;
            }
            set
            {
                if (IsConnect)
                {
                    ushort data = (ushort)value;
                    Write(FrequencyAddress, data);
                }
            }
        }

        /// <summary>
        /// 占空比(1-100)
        /// </summary>
        public int DutyRatio
        {
            get
            {
                if (IsConnect)
                {
                    ushort[] data;
                    if (Read(DutyRatioAddress, 1, out data))
                    {
                        return data[0];
                    }
                }
                return -1;
            }
            set
            {
                if (IsConnect)
                {
                    ushort data = (ushort)value;
                    Write(DutyRatioAddress, data);
                }
            }
        }

        /// <summary>
        /// 设置输出状态
        /// </summary>
        /// <param name="number">输出引脚编号</param>
        /// <param name="isEnable">输出状态</param>
        public void SetOutput(int number, bool isEnable)
        {
            if (IsConnect)
            {
                ushort data = (ushort)(isEnable ? 0xFF00 : 0x0000);
                Write(SwitchStatusAddress, data);
            }
        }

        /// <summary>
        /// 获取输出状态
        /// </summary>
        /// <param name="number">输出引脚编号</param>
        /// <returns>输出状态</returns>
        public bool GetOutput(int number)
        {
            if (IsConnect)
            {
                ushort[] data;
                if (Read(SwitchStatusAddress, 1, out data))
                {
                    return data[0] != 0;
                }
            }
            return false;
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~Power()
        // {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
