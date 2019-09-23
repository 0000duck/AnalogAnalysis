#pragma once

#include <iostream>
#include <vector>

//��ֵ�˲�
extern bool MeanFilter(double* source, double* destination, int length, int kernelSize);

//���ұ��ص�
extern bool FindEdge1(double* source, int length, double minThreshold, double maxThreshold, std::vector<int>& risingIndexs, std::vector<int>& fallingIndexs);

//���ұ��ص�,�������
extern bool FindEdge2(double* source, int length, double minThreshold, double maxThreshold, std::vector<int>& edgeIndexs, bool& isRisingFirst);

//��ֵ���
extern bool CheckInterpolate(std::vector<int> edgeIndexs);

//���ݲ�ֵ
extern bool Interpolate(std::vector<int> risingIndexs, std::vector<int> fallingIndexs, std::vector<int>& edgeIndexs, bool& isRisingFirst);

//��������
extern bool AnalysePulseData(std::vector<int> edgeIndexs, bool isRisingFirst, int sampleRate, std::vector<double>& frequency, std::vector<double>& dutyRatio);

//���Ƶ����Ч��
extern bool CheckFrequency(std::vector<double> frequency, double minFrequency, double maxFrequency, int ignoreCount);

//���ռ�ձ���Ч��
extern bool CheckDutyRatio(std::vector<double> dutyRatio, double minDutyRatio, double maxDutyRatio, int ignoreCount);

//���ұ��ص�(��FindEdge1�ı�׼�ӿڷ�װ)
extern bool FindEdge(double* source, int length, double minThreshold, double maxThreshold, int** risingIndexArray, int** fillingIndexArray, int* risingCount, int* fallingCount);

//�ͷ�ָ���ڴ���Դ
extern void FreeIntPtr(void* intPtr);