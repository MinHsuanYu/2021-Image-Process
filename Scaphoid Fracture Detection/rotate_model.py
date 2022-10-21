import torch
import torch.nn as nn
import torch.nn.functional as F
import torchvision.models as models
from torchsummary import summary
from torch.autograd import Variable
class Step2(nn.Module):
    def __init__(self):
        super(Step2,self).__init__()
        #input conv
        self.conv0 = nn.Conv2d(3, 32, 7, stride=2, padding=3, bias=False)
        self.bn0 = nn.BatchNorm2d(32)
        #block 1
        self.conv1_1 = nn.Conv2d(32, 32, 3, padding=1, bias=False)
        self.bn1_1 = nn.BatchNorm2d(32)
        self.conv1_2 = nn.Conv2d(32, 32, 3, padding=1, bias=False)
        self.bn1_2 = nn.BatchNorm2d(32)
        #block 2
        self.conv2_1 = nn.Conv2d(32, 64, 3, stride=2, padding=1, bias=False)
        self.bn2_1 = nn.BatchNorm2d(64)
        self.conv2_2 = nn.Conv2d(64, 64, 3, padding=1, bias=False)
        self.bn2_2 = nn.BatchNorm2d(64)
        self.concat2 = nn.Conv2d(32, 64, 1, stride=2)
        #block 3
        self.conv3_1 = nn.Conv2d(64, 128, 3, stride=2, padding=1, bias=False)
        self.bn3_1 = nn.BatchNorm2d(128)
        self.conv3_2 = nn.Conv2d(128, 128, 3, padding=1, bias=False)
        self.bn3_2 = nn.BatchNorm2d(128)
        self.concat3 = nn.Conv2d(64, 128, 1, stride=2)
        #block 4
        self.conv4_1 = nn.Conv2d(128, 256, 3, stride=2, padding=1, bias=False)
        self.bn4_1 = nn.BatchNorm2d(256)
        self.conv4_2 = nn.Conv2d(256, 256, 3, padding=1, bias=False)
        self.bn4_2 = nn.BatchNorm2d(256)
        self.concat4 = nn.Conv2d(128, 256, 1, stride=2)
        
        self.feature = nn.Linear(4096, 1024)

        self.maxpool = nn.MaxPool2d(3, stride=2, padding=1)
        self.maxpool_v = nn.MaxPool2d(2, stride=(2,1), padding=0)
        self.relu = nn.ReLU(inplace=True)

        self.rgs = nn.Linear(1024, 1)
    
    def forward(self, x):
        x = x.permute(0, 3, 1, 2) #axis1 = channel
        x_in = F.relu(self.bn0(self.conv0(x)))

        x_1_1 = F.relu(self.bn1_1(self.conv1_1(x_in)))
        x_1_2 = F.relu(self.bn1_2(self.conv1_2(x_1_1)))
        
        x_2_1 = F.relu(self.bn2_1(self.conv2_1(x_1_2)))
        x_2_2 = F.relu(self.bn2_2(self.conv2_2(x_2_1)))
        x_res2 = self.concat2(x_1_2)
        x_concat2 = torch.add(x_2_2, x_res2)
        
        x_3_1 = F.relu(self.bn3_1(self.conv3_1(x_concat2)))
        x_3_2 = F.relu(self.bn3_2(self.conv3_2(x_3_1)))
        x_res3 = self.concat3(x_concat2)
        x_concat3 = torch.add(x_3_2, x_res3)
        
        x_4_1 = F.relu(self.bn4_1(self.conv4_1(x_concat3)))
        x_4_2 = F.relu(self.bn4_2(self.conv4_2(x_4_1)))
        x_res4 = self.concat4(x_concat3)
        res_out = torch.add(x_4_2, x_res4)
        res_out = res_out.flatten(start_dim=1)
        feature = self.feature(res_out)
        rgs = self.rgs(feature)
        return rgs
