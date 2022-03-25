import { Component, ViewChildren  } from '@angular/core';
import { AlertController, ModalController, PopoverController } from '@ionic/angular';
import { ColectionEditPage } from '../colection-edit/colection-edit.page';
import { FirebaseApp } from '@angular/fire';
import { Colection } from '../class/colection';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: 'home.page.html',
  styleUrls: ['home.page.scss'],
})
export class HomePage {
  colectionEdit = ColectionEditPage;
  colecList: Colection[] = [];
  favouriteList: Colection[] = [];
  favouritedCount = 0;
  clickedCol: number;
  clickedElement = undefined;

  constructor(
    private modalController: ModalController,
    public alertController: AlertController,
    private fbApp: FirebaseApp,
    public router: Router
    ) {}

  ngOnInit() {
    // this.fbApp.database().ref('nome').once('value').then((snapshot) => {
    //   console.log(snapshot.val());
    // });
  }

  async editCollection(ind = -1){
    let colec = null;
    if(ind >= 0)
      colec = this.colecList[ind];
    const modal = await this.modalController.create({
      component: this.colectionEdit,
      cssClass: 'colectionEditPageClass',
      componentProps: {
        newColection: colec,
        colecInd: ind
      }
    });

    modal.onDidDismiss().then((res) => {
      console.log(res.data.data.obj);
      if(res.data.data.obj !== null){
        if(res.data.data.ind >= 0)
          this.colecList[res.data.data.ind] = res.data.data.obj;
        else
          this.colecList.push(res.data.data.obj);
      }

    });

    return await modal.present();
  }

  activeDetails(ind){
    document.getElementsByClassName('colecItem')[ind].classList.add('opened');
  }

  unactiveDetails(ind){
    document.getElementsByClassName('colecItem')[ind].classList.remove('opened');
  }

  async deleteCol(ind){
    const alert = await this.alertController.create({
      cssClass: 'dismissPopUp',
      header: 'Excluir coleção',
      message: 'Você tem certeza que deseja excluir esta coleção? As alterações serão perdidas e não poderão ser resgatadas.',
      backdropDismiss: false,
      buttons: [{text: 'Cancelar', role: 'dismiss'}, 'Excluir']
    });

    await alert.present();
    const { role } = await alert.onDidDismiss();
    if(role !== 'dismiss')
      this.colecList.splice(ind, 1);
  }

  duplicateCol(ind){
    this.colecList.splice(ind, 0, this.colecList[ind]);
  }

  favouriteCol(ind){
    if(this.favouriteList.includes(this.colecList[ind])){
      this.favouriteList.splice(this.favouriteList.indexOf(this.colecList[ind]), 1);
      this.colecList[ind].favourited = false;
      this.favouritedCount--;
    }
    else{
      this.favouriteList.push(this.colecList[ind]);
      this.colecList[ind].favourited = true;
      this.favouritedCount++;
    }
  }

  testEvent(event){
    event.srcElement.classList.add('clicked');
  }

  popoverOpen(event){
    if(event !== undefined){
      this.clickedElement = event.srcElement.parentElement.parentElement.parentElement;
      this.clickedElement.classList.add('clicked');
    }
    else{
      this.clickedElement.classList.remove('clicked');
    }
  }
}
