import { Component, Inject, ViewEncapsulation, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import {MatInputModule} from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { StoragesApi } from '../../../../services/storages.api';
import { AwsRegions } from '../../../../services/aws-regions';
import { RegionInputComponent } from '../../../../shared/region-input/region-input.component';
import { SecureInputDirective } from '../../../../shared/secure-input.directive';

@Component({
    selector: 'app-edit-aws-storage',
    imports: [
        FormsModule,
        MatFormFieldModule,
        MatInputModule,
        ReactiveFormsModule,
        MatButtonModule,
        RegionInputComponent,
        SecureInputDirective
    ],
    templateUrl: './edit-aws-storage.component.html',
    styleUrl: './edit-aws-storage.component.scss',
    encapsulation: ViewEncapsulation.None
})
export class EditAwsStorageComponent{
    isLoading = signal(false);
    couldNotConnect = signal(false);
    isUrlInvalid = signal(false);

    awsRegions = AwsRegions.S3();

    accessKey = new FormControl('', [Validators.required]);    
    secretAccessKey = new FormControl('', [Validators.required]);    
    region = new FormControl('', [Validators.required]);       

    formGroup: FormGroup;
    wasSubmitted = signal(false);
      

    constructor(
        private _storagesApi: StoragesApi,
        public dialogRef: MatDialogRef<EditAwsStorageComponent>,
        @Inject(MAT_DIALOG_DATA) public data: {storageExternalId: string}) {    
            
        this.formGroup = new FormGroup({
            accessKey: this.accessKey,
            secretAccessKey: this.secretAccessKey,
            region: this.region
        });
    }

    async onCreateStorage() {
        this.wasSubmitted.set(true);

        if(!this.formGroup.valid)
            return;

        try {
            this.isLoading.set(true);

            const result = await this._storagesApi.updateAwsS3StorageDetails(this.data.storageExternalId, {
                accessKey: this.accessKey.value!,
                secretAccessKey: this.secretAccessKey.value!,
                region: this.region.value!
            });

            this.dialogRef.close();
        } catch (err: any) {
            if (err.error.code === 'storage-connection-failed') {
                this.couldNotConnect.set(true);            
            } else {
                console.error(err);
            }
        } finally {
            this.isLoading.set(false);
        }
    }

    cancel() {
        this.dialogRef.close();
    }
}
