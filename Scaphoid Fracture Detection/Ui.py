#UI
import sys
from rotate_model import Step2
from PyQt5.QtWidgets import QDialog, QApplication, QWidget
from  final import Ui_Dialog
app = QApplication(sys.argv)
form = QWidget()
window = Ui_Dialog()
window.setupUi(form)
form.show
sys.exit(app.exec_())