from re import X
from typing import AnyStr
from PyQt5 import QtCore, QtGui, QtWidgets
import PyQt5
from PyQt5.QtWidgets import QFileDialog
from PyQt5.QtGui import QPixmap, QImage
import os
import cv2
from numpy.core.fromnumeric import mean
import torch
import json
import csv
import numpy as np
import math
from torch.autograd.grad_mode import set_grad_enabled

def area(p):
    w = abs(p[0] - p[2])
    h = abs(p[1] - p[3])
    return w * h
def inter(p1, p2):
    w = min(p1[2], p2[2]) - max(p1[0], p2[0])
    h = min(p1[3], p2[3]) - max(p1[1], p2[1])
    return w * h

class Ui_Dialog(object):
    def __init__(self):
        super(Ui_Dialog, self).__init__()
        self.cwd = os.getcwd()
        self.Rmodel = torch.load('models/rotate.pt')
        self.model = torch.hub.load('ultralytics/yolov5', 'custom', path='models/Cross2/best.pt')
        self.json_path = "Scaphoid/Annotations/Scaphoid_Slice"
        self.csv_path = "Scaphoid/Annotations/Fracture_Coordinate"
    
    def setupUi(self, Dialog):
        Dialog.setObjectName("Dialog")
        Dialog.resize(920, 800)
        self.selectFolder = QtWidgets.QPushButton(Dialog)
        self.selectFolder.setGeometry(QtCore.QRect(80, 40, 160, 40))
        font = QtGui.QFont()
        font.setFamily("Arial")
        font.setPointSize(10)
        self.selectFolder.setFont(font)
        self.selectFolder.setObjectName("SelectFolder")
        self.Detect_1 = QtWidgets.QPushButton(Dialog)
        self.Detect_1.setGeometry(QtCore.QRect(360, 40, 160, 40))
        font = QtGui.QFont()
        font.setFamily("Arial")
        font.setPointSize(10)
        self.Detect_1.setFont(font)
        self.Detect_1.setObjectName("Detect_1")
        self.Detect_2 = QtWidgets.QPushButton(Dialog)
        self.Detect_2.setGeometry(QtCore.QRect(660, 40, 180, 40))
        font = QtGui.QFont()
        font.setFamily("Arial")
        font.setPointSize(10)
        self.Detect_2.setFont(font)
        self.Detect_2.setObjectName("Detect_2")
        self.hand_ori = QtWidgets.QLabel(Dialog)
        self.hand_ori.setGeometry(QtCore.QRect(40, 100, 240, 360))
        self.hand_ori.setTextFormat(QtCore.Qt.AutoText)
        self.hand_ori.setScaledContents(True)
        self.hand_ori.setObjectName("hand_ori")
        self.hand_detect = QtWidgets.QLabel(Dialog)
        self.hand_detect.setGeometry(QtCore.QRect(320, 100, 240, 360))
        self.hand_detect.setScaledContents(True)
        self.hand_detect.setObjectName("hand_detect")
        self.label = QtWidgets.QLabel(Dialog)
        self.label.setGeometry(QtCore.QRect(680, 240, 120, 60))
        font = QtGui.QFont()
        font.setFamily("Arial")
        font.setPointSize(10)
        self.label.setFont(font)
        self.label.setObjectName("label")
        self.label_2 = QtWidgets.QLabel(Dialog)
        self.label_2.setGeometry(QtCore.QRect(680, 400, 120, 60))
        font = QtGui.QFont()
        font.setFamily("Arial")
        font.setPointSize(10)
        self.label_2.setFont(font)
        self.label_2.setObjectName("label_2")
        self.label_3 = QtWidgets.QLabel(Dialog)
        self.label_3.setGeometry(QtCore.QRect(680, 560, 120, 120))
        font = QtGui.QFont()
        font.setFamily("Arial")
        font.setPointSize(10)
        self.label_3.setFont(font)
        self.label_3.setObjectName("label_3")
        self.Sca_ori = QtWidgets.QLabel(Dialog)
        self.Sca_ori.setGeometry(QtCore.QRect(80, 560, 128, 128))
        self.Sca_ori.setScaledContents(True)
        self.Sca_ori.setObjectName("Sca_ori")
        self.Sca_detect = QtWidgets.QLabel(Dialog)
        self.Sca_detect.setGeometry(QtCore.QRect(280, 560, 128, 128))
        self.Sca_detect.setScaledContents(True)
        self.Sca_detect.setObjectName("Sca_detect")
        self.Sca_heat = QtWidgets.QLabel(Dialog)
        self.Sca_heat.setGeometry(QtCore.QRect(480, 560, 47, 12))
        self.Sca_heat.setObjectName("Sca_heat")
        self.label_7 = QtWidgets.QLabel(Dialog)
        self.label_7.setGeometry(QtCore.QRect(120, 700, 47, 20))
        font = QtGui.QFont()
        font.setFamily("Arial")
        font.setPointSize(10)
        self.label_7.setFont(font)
        self.label_7.setObjectName("label_7")
        self.label_8 = QtWidgets.QLabel(Dialog)
        self.label_8.setGeometry(QtCore.QRect(400, 700, 47, 20))
        self.label9 = QtWidgets.QLabel(Dialog)
        self.label9.setGeometry(QtCore.QRect(500, 740, 60, 20))
        font = QtGui.QFont()
        font.setFamily("Arial")
        font.setPointSize(10)
        self.label9.setFont(font)
        self.label9.setObjectName("label9")
        font = QtGui.QFont()
        font.setFamily("Arial")
        font.setPointSize(10)
        self.label_8.setFont(font)
        self.label_8.setObjectName("label_8")
        self.ImageSlider = QtWidgets.QSlider(Dialog)
        self.ImageSlider.setGeometry(QtCore.QRect(90, 740, 360, 22))
        self.ImageSlider.setOrientation(QtCore.Qt.Horizontal)
        self.ImageSlider.setObjectName("ImageSlider")

        self.Detect_2.clicked.connect(self.DetectFracture)
        self.selectFolder.clicked.connect(self.SelectFolder)
        self.Detect_1.clicked.connect(self.DetectScaphoid)
        self.ImageSlider.valueChanged.connect(self.UpdateResult)
        self.retranslateUi(Dialog)
        QtCore.QMetaObject.connectSlotsByName(Dialog)
        Dialog.show()

    def retranslateUi(self, Dialog):
        _translate = QtCore.QCoreApplication.translate
        Dialog.setWindowTitle(_translate("Dialog", "Dialog"))
        self.selectFolder.setText(_translate("Dialog", "Select Image Folder"))
        self.Detect_1.setText(_translate("Dialog", "Detect Scaphoid"))
        self.Detect_2.setText(_translate("Dialog", "Classify and Detect Fracture"))
        self.hand_ori.setText(_translate("Dialog", ""))
        self.hand_detect.setText(_translate("Dialog", ""))
        self.label.setText(_translate("Dialog", "Type:\nPredict:"))
        self.label_2.setText(_translate("Dialog", "Current Image:\nEvaluation Metric:"))
        self.label_3.setText(_translate("Dialog", "Folder(Mean):\nEvaluation Metric:"))
        self.Sca_ori.setText(_translate("Dialog", ""))
        self.Sca_detect.setText(_translate("Dialog", ""))
        self.Sca_heat.setText(_translate("Dialog", ""))
        self.label_7.setText(_translate("Dialog", "image"))
        self.label_8.setText(_translate("Dialog", "result"))
        self.label9.setText(_translate("Dialog", "0 / 0"))
        self.ImageSlider.setTickPosition(QtWidgets.QSlider.TicksBelow)
        self.ImageSlider.setTickInterval(1)
    
    def setImg(self, window, img):
        if img is None:
            window.hide()
        else:
            window.show()
            w,h,ch = img.shape
            qimg = QImage(img, h, w, 3*h, QImage.Format_RGB888) 
            img = QPixmap(qimg)
            window.setPixmap(img)
    
    def UpdateResult(self):
        #Ui enable
        self.hand_ori.show()
        self.ImageSlider.show()
        #select Img and show
        choose = self.ImageSlider.value()
        self.label9.setText(str(choose) + "/" + str(len(self.img_list)))
        self.img_ori = self.origin_imgs[choose-1]
        self.setImg(self.hand_ori, self.img_ori)
        #get img name
        self.img_name = self.img_list[choose-1]
        self.img_name = self.img_name[0:-4] #delete '.jpg'
    
    def step1(self):
        self.step1_imgs = []
        self.step1_imgs_crop = []
        self.step2_imgs = []
        self.labels = []
        self.fra_labels = []
        self.fra_gt = []
        self.IOUs = []
        count = len(self.img_list)
        #detect scaphoid     
        self.step1_result = self.model(self.origin_imgs)
        self.step1_result = self.step1_result.pandas().xyxy
        for index in range(0, count):
            img = self.origin_imgs[index].copy()
            img2 = img.copy() 
            img3 = img.copy()
            current = self.step1_result[index]
            Frac = False
            #filter difference class & info
            normal = current[current['class']==0]
            abnormal = current[current['class']==1]
            frature = current[current['class']==2]
            if normal.empty == False and abnormal.empty == False:
                normal = normal[normal['confidence'] == max(normal['confidence'])]
                abnormal = abnormal[abnormal['confidence'] == max(abnormal['confidence'])]
                normal = next(normal.iterrows())[1]
                abnormal = next(abnormal.iterrows())[1]
                #detect normal
                if normal['confidence'] > abnormal['confidence']:
                    self.labels.append(0)
                    xyxy = normal
                #detect abnormal
                else:
                    self.labels.append(1)
                    xyxy = abnormal
                    Frac = True
            #detect normal
            elif normal.empty == False:
                self.labels.append(0)
                normal = normal[normal['confidence'] == max(normal['confidence'])]
                normal = next(normal.iterrows())[1]
                xyxy = normal
            #detect abnormal
            elif abnormal.empty == False:
                self.labels.append(1)
                abnormal = abnormal[abnormal['confidence'] == max(abnormal['confidence'])]
                abnormal = next(abnormal.iterrows())[1]
            
                xyxy = abnormal
                Frac = True
            else:
                xyxy = None
            #draw predict and crop predict
            if xyxy is not None:
                img2 = img2[int(xyxy['ymin']):int(xyxy['ymax']), int(xyxy['xmin']):int(xyxy['xmax'])]
                img2 = np.array(img2)
                cv2.rectangle(img, (int(xyxy['xmin']), int(xyxy['ymin'])), (int(xyxy['xmax']), int(xyxy['ymax'])), (0, 0, 255), 3)
            #draw groudtruth
            with open(os.path.join(self.json_path, self.img_list[index][0:-4] + '.json')) as file:
                data = json.load(file)
                _xyxy = data[0]['bbox']
                cv2.rectangle(img, (int(_xyxy[0]), int(_xyxy[1])), (int(_xyxy[2]), int(_xyxy[3])), (255, 0, 0), 3)
            if xyxy is None:
                self.IOUs.append(0)
            else:
                p1 = [int(xyxy['xmin']), int(xyxy['ymin']), int(xyxy['xmax']), int(xyxy['ymax'])]
                p2 = [int(_xyxy[0]), int(_xyxy[1]), int(_xyxy[2]), int(_xyxy[3]),]
                A = area(p1)
                B = area(p2)
                interA_B = inter(p1, p2)
                IOU = interA_B / (A+B-interA_B)
                self.IOUs.append(IOU)
            
            if Frac == True and frature.empty == False:
                self.fra_labels.append(1)
                frature = frature[frature['confidence'] == max(frature['confidence'])]
                frature = next(frature.iterrows())[1]
                input_img = img2.copy()
                input_img = cv2.resize(input_img, (64, 64))
                input_img = np.array([input_img], dtype=np.float32)
                input_img = torch.from_numpy(input_img).cuda()
                #draw predict
                theta = self.Rmodel(input_img).item() * 360
                theta = theta * math.pi /180
                x0, y0, x1, y1 = int(frature['xmin']), int(frature['ymin']), int(frature['xmax']), int(frature['ymax'])
                cx, cy = (x0+x1)/2 , (y0+y1)/2
                points = [[x0, y0], [x0, y1], [x1, y1], [x1, y0]]
                pts = []
                for index in range(len(points)):
                    x, y = points[index]
                    x -= cx
                    y -= cy
                    out_x = x*math.cos(theta) - y*math.sin(theta)
                    out_y = x*math.sin(theta) + y*math.cos(theta)
                    out_x += cx
                    out_y += cy
                    pts.append([out_x, out_y])
                pts = np.array(pts, dtype=np.int32)
                pts.reshape((-1, 1, 2))
                img3 = cv2.polylines(img3, pts=[pts], isClosed=True, color=(0,0,255), thickness=3)
            else: self.fra_labels.append(0)
            #draw fracture gt
            gt_path = os.path.join("Scaphoid/Annotations/Fracture_Coordinate", self.img_list[index][0:-4]+'.csv')
            if os.path.isfile(gt_path):
                self.fra_gt.append(1)
                with open(gt_path) as csvfile:
                    data = csv.reader(csvfile)
                    next(data)
                    data = next(data)
                    cx, cy, w, h, angle = np.array(data,dtype=np.int32)
                    theta = angle * math.pi /180
                    points = [[-w/2, h/2], [w/2, h/2], [w/2, -h/2], [-w/2, -h/2]]
                    ori_x , ori_y = int(_xyxy[0]), int(_xyxy[1])
                    pts = []
                    for index in range(len(points)):
                        x, y = points[index]
                        out_x = x*math.cos(theta) - y*math.sin(theta)
                        out_y = x*math.sin(theta) + y*math.cos(theta)
                        out_x += (cx + ori_x)
                        out_y += (cy + ori_y)
                        pts.append([out_x, out_y])
                    pts = np.array(pts, dtype=np.int32)
                    pts.reshape((-1, 1, 2))
                    img3 = cv2.polylines(img3, pts=[pts], isClosed=True, color=(255,0), thickness=3)
            else:self.fra_gt.append(0)
            img3 = img3[int(_xyxy[1]):int(_xyxy[3]), int(_xyxy[0]):int(_xyxy[2])]
            img3 = np.array(img3)
            self.step1_imgs.append(img)
            self.step1_imgs_crop.append(img2)
            self.step2_imgs.append(img3)


    def Testing(self):
        self.step1()
    
    def Metric(self):
        mean_IOU = np.mean(self.IOUs)
        TP, TN, FP, FN = [0, 0, 0, 0]
        for i in range(len(self.labels)):
            if self.labels[i] == 1:
                if self.fra_gt[i] == 1:
                    TP += 1
                else: FP += 1
            else:
                if self.fra_gt[i] == 1:
                    FN += 1
                else: TN += 1
        if TP+FN == 0:
            recall = 0
        else: recall = round(TP/(TP+FN), 6)
        if TP+FP == 0:
            precision = 0
        else: precision = round(TP/(TP+FP), 6)
        F1 = round(2 * (recall*precision)/(recall+precision), 6)
        output = "Folder(Mean):\nEvaluation Metric:"
        output += "\nIOU: " + str(mean_IOU)
        output += "\nAccuracy: " + str(round((TP + TN) / (TP + FP + TN + FN), 6))
        output += "\nRecall: " + str(recall)
        output += "\nprecision: " + str(precision)
        output += "\nF1-Score: " + str(F1)
        self.label_3.setText(output)
        
    def SelectFolder(self):
        self.chooseFolder = QFileDialog.getExistingDirectory(None, "select folder", self.cwd)
        self.img_list = os.listdir(self.chooseFolder)
        count = len(self.img_list)
        self.origin_imgs = []
        print("Loading Image...")
        for img_name in self.img_list:
            img = cv2.imread(os.path.join(self.chooseFolder, img_name))
            self.origin_imgs.append(img)
        print("Load", count, "imgs...")
        self.ImageSlider.setRange(1, count)
        self.ImageSlider.setValue(1)
        self.Testing()
        self.Metric()

    def DetectScaphoid(self):
        choose = self.ImageSlider.value() - 1
        self.setImg(self.hand_detect, self.step1_imgs[choose])
        self.setImg(self.Sca_ori, self.step1_imgs_crop[choose])
        self.setImg(self.Sca_detect, self.step2_imgs[choose])
    
    def DetectFracture(self):
        choose = self.ImageSlider.value() - 1
        if self.labels[choose] == 0:
            pred = "Normal"
        else: pred = "Fracture"
        if self.fra_gt[choose] == 0:
            gt = "Normal"
        else: gt = "Fracture"
        self.label.setText("Type: " + gt + "\nPredict: " + pred + '(' + str(self.fra_labels[choose]))
        self.label_2.setText("Current Image:\nIOU: " + str(round(self.IOUs[choose], 6)))
        